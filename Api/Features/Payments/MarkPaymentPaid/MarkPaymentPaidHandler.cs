using Api.Data;
using Api.Domain.Orders;
using Api.Domain.Payments;
using Contracts.Payments.Commands;
using Contracts.Payments.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.Payments.MarkPaymentPaid;

public class MarkPaymentPaidHandler
{
    public static async Task<object> Handle(
        MarkPaymentPaidCommand command,
        OrderDbContext dbContext,
        ILogger<MarkPaymentPaidHandler> logger,
        CancellationToken ct)
    {
        var order = await dbContext.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

        if (order is null)
            throw new KeyNotFoundException($"Order with ID '{command.OrderId}' was not found.");

        if (order.Status == OrderStatus.Completed)
            throw new InvalidOperationException($"Order with ID '{command.OrderId}' is already completed.");

        // Strict invariant: A payment attempt MUST already exist
        var payment = command.PaymentId.HasValue
            ? order.Payments.FirstOrDefault(p => p.Id == command.PaymentId.Value)
            : order.Payments.LastOrDefault();

        if (payment is null)
            throw new InvalidOperationException($"Cannot mark payment as paid: No active payment attempt exists for Order '{command.OrderId}'. Initiate payment first.");

        var transactionId = command.GatewayTransactionId 
            ?? payment.GatewayTransactionId 
            ?? $"manual_tx_{Guid.NewGuid():N}";

        // 1. Acknowledge money capture on the ledger
        payment.MarkSuccess(transactionId);

        // 2. If the order timed out or was canceled: Auto-compensate via Refund
        if (order.Status == OrderStatus.TimedOut || order.Status == OrderStatus.Canceled)
        {
            logger.LogWarning("Payment {PaymentId} arrived for Order {OrderId} in '{Status}' state. Initiating automatic refund.",
                payment.Id, order.Id, order.Status);

            return new RefundPaymentCommand(
                OrderId: order.Id,
                PaymentId: payment.Id,
                Amount: payment.Amount,
                Reason: $"Payment arrived late while order was in '{order.Status}' state."
            );
        }

        // 3. Normal Path: Order is pending, complete the saga
        logger.LogInformation("Payment {PaymentId} marked as Paid for Order {OrderId} (TxId: {TxId}).",
            payment.Id, order.Id, transactionId);

        return new PaymentCompleted(order.Id, payment.Id, payment.Amount);
    }
}
