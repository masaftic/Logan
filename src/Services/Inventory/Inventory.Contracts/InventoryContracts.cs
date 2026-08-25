namespace Inventory.Contracts;

public record StockReserved(Guid OrderId, Guid ItemId, int Quantity, DateTime ReservedAtUtc);
public record StockExhausted(Guid OrderId, Guid ItemId, int RequestedQuantity);
