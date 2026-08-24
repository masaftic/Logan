namespace Contracts;

public record ProcessPayment(
    Guid OrderId,
    decimal Amount
);
