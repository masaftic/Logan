using Api.Data;
using Api.Features.Payments.Services;
using Contracts.Payments.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Features.Payments.RefundPayment;

public class RefundPaymentHandler
{
    public static async Task Handle(
        RefundPaymentCommand command,
        OrderDbContext dbContext,
        IPaymentGateway gateway,
        ILogger<RefundPaymentHandler> logger,
        CancellationToken ct)
    {
        logger.LogWarning("Handling refund for Payment {PaymentId} (Order: {OrderId}, Amount: {Amount:C}). Reason: {Reason}",
            command.PaymentId, command.OrderId, command.Amount, command.Reason);

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == command.PaymentId, ct);

        if (payment is null)
        {
            logger.LogError("Cannot refund: Payment {PaymentId} was not found.", command.PaymentId);
            return;
        }

        var txId = payment.GatewayTransactionId ?? $"tx_{payment.Id:N}";

        var result = await gateway.RefundPaymentAsync(
            new RefundRequest(txId, command.Amount, command.Reason),
            ct);

        if (!result.IsSuccess)
        {
            logger.LogError("Refund failed for Payment {PaymentId}: {ErrorMessage}",
                payment.Id, result.ErrorMessage);
            return;
        }

        payment.MarkRefunded();
        logger.LogInformation("Payment {PaymentId} successfully marked as Refunded.", payment.Id);
    }
}
