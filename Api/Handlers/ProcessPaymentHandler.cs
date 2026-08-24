using Contracts;
using Microsoft.Extensions.Logging;

namespace Api.Handlers;

public class ProcessPaymentHandler
{
    public static async Task<object> Handle(
        ProcessPayment command,
        ILogger<ProcessPaymentHandler> logger,
        CancellationToken ct)
    {
        logger.LogInformation("Processing payment for Order {OrderId} (Amount: {Amount:C}). Simulating gateway call...",
            command.OrderId, command.Amount);

        // Simulate external payment gateway latency
        await Task.Delay(2000, ct);


        // Simulate network drop
        if (Random.Shared.Next(0, 2) == 0)
        {
            // TODO: solve this
            // throw new Exception("Payment gateway unavailable.");
        }

        // Simulated failure rule: amount > $1000 fails
        if (command.Amount > 1000)
        {
            logger.LogWarning("Payment failed for Order {OrderId}: Amount exceeds threshold ($1000).", command.OrderId);
            return new PaymentFailed(command.OrderId, "Order amount exceeds limit of $1,000.");
        }

        var paymentId = Guid.NewGuid();
        logger.LogInformation("Payment {PaymentId} completed successfully for Order {OrderId}.", paymentId, command.OrderId);

        return new PaymentCompleted(command.OrderId, paymentId, command.Amount);
    }
}
