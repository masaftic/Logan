using Ordering.Contracts.DTOs;

namespace Ordering.Contracts.Commands;

public record SubmitOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequestDto> Items
);
