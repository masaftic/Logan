namespace Payment.Contracts.Events;

public record PaymentFailedEvent(
    Guid OrderId,
    string ErrorCode,
    string DeclineReason,
    DateTime FailedAtUtc
);
