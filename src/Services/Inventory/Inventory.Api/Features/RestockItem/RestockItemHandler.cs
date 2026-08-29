using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Inventory.Api.Data;
using Inventory.Api.Data.Extensions;
using Inventory.Api.Domain;
using Inventory.Api.Domain.Errors;
using Inventory.Contracts.Commands;
using Inventory.Contracts.Events;
using Wolverine;

namespace Inventory.Api.Features.RestockItem;

public static class RestockItemHandler
{
    public static async Task<Result> Handle(
        RestockItemCommand command,
        InventoryDbContext dbContext,
        IMessageBus bus,
        CancellationToken ct)
    {
        var sku = Sku.Create(command.Sku);
        var quantity = PositiveQuantity.Create(command.Quantity);

        Dictionary<Sku, StockItem> stockItems = await dbContext.LockStockItemsForUpdateAsync([sku], ct);

        if (!stockItems.TryGetValue(sku, out var stockItem))
            return InventoryErrors.StockNotFound(sku);

        var movement = stockItem.Restock(quantity, command.ReferenceId);
        dbContext.StockMovements.Add(movement);

        var restockedEvent = new StockRestockedEvent(
            sku,
            command.Quantity,
            stockItem.QuantityAvailable,
            stockItem.QuantityOnHand,
            command.ReferenceId,
            DateTime.UtcNow);

        await bus.PublishAsync(restockedEvent);

        return Result.Ok();
    }
}

