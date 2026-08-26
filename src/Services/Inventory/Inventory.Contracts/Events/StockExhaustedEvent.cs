namespace Inventory.Contracts.Events;

public record StockExhaustedEvent(
    Guid OrderId,
    string ShortfallSku,
    int RequestedQuantity,
    int AvailableQuantity
);

