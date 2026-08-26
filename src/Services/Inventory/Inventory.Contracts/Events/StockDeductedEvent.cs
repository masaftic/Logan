using Inventory.Contracts.DTOs;

namespace Inventory.Contracts.Events;

public record StockDeductedEvent(
    Guid OrderId,
    IReadOnlyList<StockReservationItemDto> DeductedItems,
    DateTime DeductedAtUtc
);

