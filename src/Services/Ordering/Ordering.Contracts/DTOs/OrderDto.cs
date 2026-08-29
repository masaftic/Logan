using Ordering.Contracts.Enums;

namespace Ordering.Contracts.DTOs;

public record OrderDto(
    Guid Id,
    Guid CustomerId,
    decimal TotalAmount,
    OrderStatusDto Status,
    IReadOnlyList<OrderItemDto> Items,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? CancelledAtUtc,
    string? CancellationReason
);
