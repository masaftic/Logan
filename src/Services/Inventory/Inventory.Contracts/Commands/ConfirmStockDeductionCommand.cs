namespace Inventory.Contracts.Commands;

public record ConfirmStockDeductionCommand(
    Guid OrderId
);

