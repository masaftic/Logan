using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Payment.Api.Data;
using Payment.Api.Data.Extensions;
using Payment.Api.Domain.Enums;
using Payment.Api.Services;
using Payment.Contracts.Events;
using Stripe;
using Wolverine;

namespace Payment.Api.Features.StripeWebhook;

public static class StripeWebhookEndpoint
{
    public static void MapStripeWebhookEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/webhook", async (
            HttpContext context,
            IStripePaymentGateway stripeGateway,
            PaymentDbContext dbContext,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var json = await new StreamReader(context.Request.Body).ReadToEndAsync(ct);
            var signatureHeader = context.Request.Headers["Stripe-Signature"].ToString();

            var eventResult = stripeGateway.ConstructWebhookEvent(json, signatureHeader);
            if (eventResult.IsError)
            {
                return Results.BadRequest(eventResult.FirstError.Description);
            }

            var stripeEvent = eventResult.Value;

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                if (stripeEvent.Data.Object is PaymentIntent intent)
                {
                    var payment = await FindPaymentRecordAsync(dbContext, intent, ct);
                    if (payment is not null && payment.Status != PaymentStatus.Succeeded)
                    {
                        payment.MarkSucceeded(DateTime.UtcNow);
                        await dbContext.SaveChangesAsync(ct);

                        var completedEvent = new PaymentCompletedEvent(
                            payment.OrderId,
                            payment.PaymentIntentId ?? intent.Id,
                            payment.Amount,
                            payment.Currency,
                            payment.CompletedAtUtc ?? DateTime.UtcNow
                        );

                        await bus.PublishAsync(completedEvent);
                    }
                }
            }
            else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
            {
                if (stripeEvent.Data.Object is PaymentIntent intent)
                {
                    var payment = await FindPaymentRecordAsync(dbContext, intent, ct);
                    if (payment is not null && payment.Status != PaymentStatus.Failed)
                    {
                        var failureReason = intent.LastPaymentError?.Message ?? "Payment failed";
                        var errorCode = intent.LastPaymentError?.Code ?? "card_declined";

                        payment.MarkFailed(failureReason);
                        await dbContext.SaveChangesAsync(ct);

                        var failedEvent = new PaymentFailedEvent(
                            payment.OrderId,
                            errorCode,
                            failureReason,
                            DateTime.UtcNow
                        );

                        await bus.PublishAsync(failedEvent);
                    }
                }
            }

            return Results.Ok();
        })
        .WithName("StripeWebhook")
        .WithSummary("Handle incoming Stripe webhooks")
        .WithTags("Payments")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<Domain.PaymentRecord?> FindPaymentRecordAsync(
        PaymentDbContext dbContext,
        PaymentIntent intent,
        CancellationToken ct)
    {
        var payment = await dbContext.Payments
            .SingleOrDefaultAsync(p => p.PaymentIntentId == intent.Id, ct);

        if (payment is null && intent.Metadata.TryGetValue("payment_id", out var paymentIdStr) && Guid.TryParse(paymentIdStr, out var paymentId))
        {
            payment = await dbContext.Payments
                .SingleOrDefaultAsync(p => p.Id == paymentId, ct);
        }

        if (payment is null && intent.Metadata.TryGetValue("order_id", out var orderIdStr) && Guid.TryParse(orderIdStr, out var orderId))
        {
            payment = await dbContext.Payments
                .WhereForOrder(orderId)
                .OrderByLatest()
                .FirstOrDefaultAsync(ct);
        }

        return payment;
    }
}
