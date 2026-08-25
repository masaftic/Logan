using Contracts.Payments.Commands;
using Contracts.Payments.DTOs;
using Wolverine;

namespace Api.Features.Payments.ReceiveWebhook;

public static class ReceiveWebhookEndpoint
{
    public static void MapReceiveWebhookEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/webhooks/payments", async (PaymentWebhookRequest webhook, IMessageBus bus) =>
        {
            await bus.InvokeAsync(new ProcessPaymentWebhookCommand(
                webhook.EventType,
                webhook.OrderId,
                webhook.GatewayTransactionId,
                webhook.Amount,
                webhook.FailureReason
            ));

            return Results.Ok(new { Received = true, Event = webhook.EventType });
        })
        .WithName("ReceivePaymentWebhook")
        .WithSummary("Receive simulated payment gateway webhooks")
        .Produces(StatusCodes.Status200OK);
    }
}
