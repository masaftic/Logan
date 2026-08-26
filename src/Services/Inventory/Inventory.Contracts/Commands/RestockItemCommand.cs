namespace Inventory.Contracts.Commands;

public record RestockItemCommand(
    string Sku,
    int Quantity,
    string? ReferenceId = null
);

