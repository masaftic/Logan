namespace Contracts;

public record PaymentCompleted(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount
);
