namespace Payment.Contracts.Events;

public record PaymentCompletedEvent(
    Guid OrderId,
    string PaymentIntentId,
    decimal Amount,
    string Currency,
    DateTime CompletedAtUtc
);
