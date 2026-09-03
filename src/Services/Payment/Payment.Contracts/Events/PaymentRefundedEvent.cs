namespace Payment.Contracts.Events;

public record PaymentRefundedEvent(
    Guid PaymentId,
    Guid OrderId,
    string RefundId,
    decimal AmountRefunded,
    decimal TotalAmountRefunded,
    DateTime RefundedAtUtc
);
