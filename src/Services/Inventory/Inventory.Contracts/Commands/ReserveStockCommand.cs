using Inventory.Contracts.DTOs;

namespace Inventory.Contracts.Commands;

public record ReserveStockCommand(
    Guid OrderId,
    IReadOnlyList<StockReservationItemDto> Items,
    int? HoldDurationMinutes = null
);

