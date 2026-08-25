namespace Contracts.Orders.Events;

public record OrderSubmitted(
    Guid OrderId,
    decimal Amount,
    DateTime CreatedAt
);
