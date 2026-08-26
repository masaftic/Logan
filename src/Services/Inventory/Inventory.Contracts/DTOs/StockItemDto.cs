namespace Inventory.Contracts.DTOs;

public record StockItemDto(
    string Sku,
    string Name,
    int QuantityAvailable,
    int QuantityReserved,
    int QuantityOnHand
);

