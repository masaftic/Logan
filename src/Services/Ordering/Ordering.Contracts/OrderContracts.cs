namespace Ordering.Contracts;

public record OrderSubmitted(Guid OrderId, Guid CustomerId, decimal Amount, DateTime CreatedAtUtc);
