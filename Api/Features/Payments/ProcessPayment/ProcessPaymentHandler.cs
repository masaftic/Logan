using Api.Data;
using Api.Domain.Payments;
using Api.Features.Payments.Services;
using Contracts.Payments.Commands;
using Contracts.Payments.Events;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Payments.ProcessPayment;

public class ProcessPaymentHandler
{
    public static async Task<object> Handle(
        ProcessPaymentCommand command,
        IPaymentGateway paymentGateway,
        OrderDbContext dbContext,
        ILogger<ProcessPaymentHandler> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Executing payment processing for Payment {PaymentId} (Order: {OrderId}, Amount: {Amount:C})...",
            command.PaymentId, command.OrderId, command.Amount);

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == command.PaymentId, ct);

        if (payment is null)
        {
            logger.LogWarning("Payment {PaymentId} not found in database. Creating fallback attempt.", command.PaymentId);
            payment = Payment.Create(command.OrderId, command.Amount, command.IdempotencyKey);
            dbContext.Payments.Add(payment);
        }

        payment.MarkProcessing();

        var result = await paymentGateway.ProcessPaymentAsync(
            new PaymentRequest(command.OrderId, command.Amount, command.IdempotencyKey), 
            ct);

        if (!result.IsSuccess)
        {
            payment.MarkFailed(result.FailureReason ?? "Payment declined by gateway.");
            return new PaymentFailed(command.OrderId, result.FailureReason ?? "Payment declined.");
        }

        payment.MarkSuccess(result.TransactionId?.ToString() ?? Guid.NewGuid().ToString());

        return new PaymentCompleted(command.OrderId, payment.Id, result.Amount);
    }
}
