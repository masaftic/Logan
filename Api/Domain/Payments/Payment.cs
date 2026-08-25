namespace Api.Domain.Payments;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? GatewayTransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    // Required by EF Core
    private Payment() { }

    public static Payment Create(Guid orderId, decimal amount, string idempotencyKey, string? gatewayTransactionId = null)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));

        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Status = PaymentStatus.Initiated,
            IdempotencyKey = idempotencyKey,
            GatewayTransactionId = gatewayTransactionId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkSuccess(string gatewayTransactionId)
    {
        if (string.IsNullOrWhiteSpace(gatewayTransactionId))
            throw new ArgumentException("Gateway transaction ID is required to mark payment as success.", nameof(gatewayTransactionId));

        if (Status == PaymentStatus.Success)
            return; // Idempotent

        if (Status == PaymentStatus.Failed)
            throw new InvalidOperationException("Cannot mark a failed payment attempt as success.");

        Status = PaymentStatus.Success;
        GatewayTransactionId = gatewayTransactionId;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason is required.", nameof(reason));

        if (Status == PaymentStatus.Success)
            throw new InvalidOperationException("Cannot mark a successful payment as failed.");

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void MarkRefunded()
    {
        if (Status != PaymentStatus.Success)
            throw new InvalidOperationException("Can only refund a successful payment.");

        Status = PaymentStatus.Refunded;
        CompletedAtUtc = DateTime.UtcNow;
    }
}
