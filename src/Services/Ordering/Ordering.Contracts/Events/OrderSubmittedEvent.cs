using Ordering.Contracts.DTOs;

namespace Ordering.Contracts.Events;

public record OrderSubmittedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    IReadOnlyList<OrderItemDto> Items,
    DateTime CreatedAtUtc
);
