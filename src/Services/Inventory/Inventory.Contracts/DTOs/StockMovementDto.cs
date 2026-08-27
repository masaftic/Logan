namespace Inventory.Contracts.DTOs;

public record StockMovementDto(
    Guid Id,
    string Sku,
    int QuantityDelta,
    int AvailableAfter,
    int ReservedAfter,
    StockMovementType Type,
    string? ReferenceId,
    DateTime CreatedAtUtc
);


public enum StockMovementType
{
    Restock = 1,
    Reservation = 2,
    Release = 3,
    FulfillmentDeduction = 4,
    Adjustment = 5
}
