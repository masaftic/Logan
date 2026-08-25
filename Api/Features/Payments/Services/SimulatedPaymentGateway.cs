using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Api.Features.Payments.Services;

public class SimulatedPaymentGateway : IPaymentGateway
{
    private readonly ILogger<SimulatedPaymentGateway> _logger;
    private static readonly ConcurrentDictionary<string, PaymentRecord> Transactions = new();
    private static readonly ConcurrentDictionary<string, PaymentIntentRecord> Intents = new();

    public SimulatedPaymentGateway(ILogger<SimulatedPaymentGateway> logger)
    {
        _logger = logger;
    }

    public Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId, 
        decimal amount, 
        CancellationToken ct = default)
    {
        var intentId = $"pi_{Guid.NewGuid():N}";
        var clientSecret = $"{intentId}_secret_{Guid.NewGuid():N}";

        var intent = new PaymentIntentRecord(intentId, clientSecret, orderId, amount, DateTime.UtcNow);
        Intents[intentId] = intent;

        _logger.LogInformation("Created PaymentIntent {IntentId} for Order {OrderId} (Amount: {Amount:C}).",
            intentId, orderId, amount);

        return Task.FromResult(new PaymentIntentResult(intentId, clientSecret, orderId, amount));
    }

    public async Task<PaymentExecutionResult> ProcessPaymentAsync(
        PaymentRequest request, 
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Contacting Payment Gateway for Order {OrderId} (Amount: {Amount:C}, IdempotencyKey: {Key})...",
            request.OrderId, request.Amount, request.IdempotencyKey);

        // 1. Idempotency Check
        if (Transactions.TryGetValue(request.IdempotencyKey, out var existing))
        {
            _logger.LogInformation(
                "Idempotent hit: Returning cached gateway result for IdempotencyKey {Key}.",
                request.IdempotencyKey);

            return new PaymentExecutionResult(
                IsSuccess: existing.Status == GatewayTransactionStatus.Success,
                TransactionId: existing.TransactionId,
                Amount: existing.Amount,
                FailureReason: existing.FailureReason,
                Status: existing.Status
            );
        }

        // 2. Simulate gateway latency
        await Task.Delay(2000, ct);

        // 3. Transient Failure Simulation
        if (request.Amount == 999m)
        {
            _logger.LogError("Payment Gateway simulated a 503 Timeout for Order {OrderId}.", request.OrderId);
            throw new HttpRequestException("Payment gateway HTTP 503: Gateway Timeout / Service Unavailable");
        }

        // 4. Decline Simulation
        if (request.Amount > 1000m)
        {
            var declinedRecord = new PaymentRecord(
                TransactionId: null,
                OrderId: request.OrderId,
                Amount: request.Amount,
                Status: GatewayTransactionStatus.Declined,
                FailureReason: "Declined: Amount exceeds single transaction limit ($1,000.00)",
                ProcessedAtUtc: DateTime.UtcNow
            );

            Transactions[request.IdempotencyKey] = declinedRecord;

            return new PaymentExecutionResult(
                IsSuccess: false,
                TransactionId: null,
                Amount: request.Amount,
                FailureReason: declinedRecord.FailureReason,
                Status: GatewayTransactionStatus.Declined
            );
        }

        // 5. Success Capture
        var transactionId = Guid.NewGuid();
        var successRecord = new PaymentRecord(
            TransactionId: transactionId,
            OrderId: request.OrderId,
            Amount: request.Amount,
            Status: GatewayTransactionStatus.Success,
            FailureReason: null,
            ProcessedAtUtc: DateTime.UtcNow
        );

        Transactions[request.IdempotencyKey] = successRecord;

        return new PaymentExecutionResult(
            IsSuccess: true,
            TransactionId: transactionId,
            Amount: request.Amount,
            FailureReason: null,
            Status: GatewayTransactionStatus.Success
        );
    }

    public Task<PaymentStatusResult> GetStatusAsync(
        string idempotencyKey, 
        CancellationToken ct = default)
    {
        if (Transactions.TryGetValue(idempotencyKey, out var record))
        {
            return Task.FromResult(new PaymentStatusResult(
                Exists: true,
                TransactionId: record.TransactionId,
                Amount: record.Amount,
                Status: record.Status
            ));
        }

        return Task.FromResult(new PaymentStatusResult(
            Exists: false,
            TransactionId: null,
            Amount: null,
            Status: GatewayTransactionStatus.NotFound
        ));
    }

    public Task<RefundResult> RefundPaymentAsync(
        RefundRequest request, 
        CancellationToken ct = default)
    {
        _logger.LogInformation("Processing Refund of {Amount:C} for Transaction {TxId}. Reason: {Reason}",
            request.Amount, request.TransactionId, request.Reason);

        var refundId = Guid.NewGuid();
        return Task.FromResult(new RefundResult(
            IsSuccess: true,
            RefundId: refundId,
            ErrorMessage: null
        ));
    }

    private record PaymentRecord(
        Guid? TransactionId,
        Guid OrderId,
        decimal Amount,
        GatewayTransactionStatus Status,
        string? FailureReason,
        DateTime ProcessedAtUtc
    );

    private record PaymentIntentRecord(
        string IntentId,
        string ClientSecret,
        Guid OrderId,
        decimal Amount,
        DateTime CreatedAtUtc
    );
}
