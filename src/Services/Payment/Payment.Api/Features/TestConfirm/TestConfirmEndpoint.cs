using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Payment.Api.Data;
using Payment.Api.Data.Extensions;
using Payment.Api.Domain.Errors;
using Payment.Api.Services;

namespace Payment.Api.Features.TestConfirm;

public static class TestConfirmEndpoint
{
    public static void MapTestConfirmEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/orders/{orderId:guid}/test-confirm", async (
            Guid orderId,
            [FromBody] TestConfirmRequest? request,
            PaymentDbContext dbContext,
            IStripePaymentGateway stripeGateway,
            CancellationToken ct) =>
        {
            var payment = await dbContext.Payments
                .WhereForOrder(orderId)
                .OrderByLatest()
                .FirstOrDefaultAsync(ct);

            if (payment is null)
            {
                return Results.NotFound(new { Error = $"Payment for order '{orderId}' was not found." });
            }

            if (string.IsNullOrEmpty(payment.PaymentIntentId))
            {
                return Results.BadRequest(new { Error = "PaymentIntent has not been created yet for this order." });
            }

            var paymentMethod = request?.PaymentMethod ?? "pm_card_visa";
            var result = await stripeGateway.ConfirmTestPaymentAsync(payment.PaymentIntentId, paymentMethod, ct);

            if (result.IsError)
            {
                return Results.BadRequest(new { Error = result.FirstError.Description });
            }

            return Results.Ok(new TestConfirmResponse(
                payment.OrderId,
                payment.PaymentIntentId,
                result.Value
            ));
        })
        .WithName("TestConfirmPayment")
        .WithSummary("Confirm a payment intent in sandbox using test cards (e.g. pm_card_visa)")
        .WithTags("Payments")
        .Produces<TestConfirmResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}

public record TestConfirmRequest(string? PaymentMethod = "pm_card_visa");
public record TestConfirmResponse(Guid OrderId, string PaymentIntentId, string StripeStatus);
