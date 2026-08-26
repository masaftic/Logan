using Inventory.Contracts.DTOs;

namespace Inventory.Contracts.Events;

public record StockReservedEvent(
    Guid OrderId,
    IReadOnlyList<StockReservationItemDto> Items,
    DateTime ReservedAtUtc
);

