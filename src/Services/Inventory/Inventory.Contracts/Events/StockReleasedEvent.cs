using Inventory.Contracts.DTOs;

namespace Inventory.Contracts.Events;

public record StockReleasedEvent(
    Guid OrderId,
    IReadOnlyList<StockReservationItemDto> ReleasedItems,
    string? Reason,
    DateTime ReleasedAtUtc
);

