namespace Contracts;

public record PaymentFailed(
    Guid OrderId,
    string Reason
);
