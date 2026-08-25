using Api.Data;
using Contracts.Payments.Commands;
using Contracts.Payments.Events;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Payments.ReceiveWebhook;

public class ReceiveWebhookHandler
{
    public async Task<object?> Handle(
        ProcessPaymentWebhookCommand webhook,
        OrderDbContext dbContext,
        ILogger<ReceiveWebhookHandler> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Processing Webhook command: {EventType} for Order {OrderId} (TxId: {TxId}).",
            webhook.EventType, webhook.OrderId, webhook.GatewayTransactionId);

        var order = await dbContext.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == webhook.OrderId, ct);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for webhook {EventType}.", webhook.OrderId, webhook.EventType);
            return null;
        }

        var payment = order.Payments
            .FirstOrDefault(p => p.GatewayTransactionId == webhook.GatewayTransactionId) 
            ?? order.Payments.LastOrDefault();

        switch (webhook.EventType.ToLowerInvariant())
        {
            case "payment_intent.succeeded":
            case "payment.success":
                payment?.MarkSuccess(webhook.GatewayTransactionId);
                return new PaymentCompleted(
                    webhook.OrderId, 
                    payment?.Id ?? Guid.NewGuid(), 
                    webhook.Amount);

            case "payment_intent.payment_failed":
            case "payment.failed":
                payment?.MarkFailed(webhook.FailureReason ?? "Gateway declined payment.");
                return new PaymentFailed(
                    webhook.OrderId, 
                    webhook.FailureReason ?? "Payment failed via webhook.");

            default:
                logger.LogWarning("Unhandled webhook event type: {EventType}", webhook.EventType);
                return null;
        }
    }
}
