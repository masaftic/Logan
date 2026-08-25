namespace Contracts.Payments.Events;

public record PaymentCompleted(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount
);

public record PaymentFailed(
    Guid OrderId,
    string Reason
);
