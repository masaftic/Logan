namespace Contracts;

public record OrderSubmitted(
    Guid OrderId,
    decimal Amount,
    DateTime CreatedAt
);
