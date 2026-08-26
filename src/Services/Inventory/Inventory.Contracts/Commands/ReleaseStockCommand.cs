namespace Inventory.Contracts.Commands;

public record ReleaseStockCommand(
    Guid OrderId,
    string? Reason = null
);

