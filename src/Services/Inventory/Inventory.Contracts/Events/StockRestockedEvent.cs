namespace Inventory.Contracts.Events;

public record StockRestockedEvent(
    string Sku,
    int QuantityAdded,
    int NewQuantityAvailable,
    int NewQuantityOnHand,
    string? ReferenceId,
    DateTime RestockedAtUtc
);

