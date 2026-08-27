using BuildingBlocks.Common.Results;
using Inventory.Api.Domain.ValueObjects;
using Inventory.Api.Data;
using Inventory.Api.Data.Extensions;
using Inventory.Api.Domain;
using Inventory.Api.Domain.Enums;
using Inventory.Contracts.Commands;
using Inventory.Contracts.DTOs;
using Inventory.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Inventory.Api.Features.ReleaseStock;

public class ReleaseStockHandler
{
    public static async Task<Result> Handle(
        ReleaseStockCommand command,
        InventoryDbContext dbContext,
        IMessageBus bus,
        ILogger<ReleaseStockHandler> logger,
        CancellationToken ct)
    {
        List<StockReservation> activeReservations = await dbContext.StockReservations
            .WhereActiveForOrder(command.OrderId)
            .ToListAsync(ct);

        if (activeReservations.Count == 0)
        {
            return Result.Ok();
        }

        List<string> skus = [.. activeReservations
            .Select(r => r.Sku.Trim().ToUpperInvariant())
            .Distinct() ];

        Dictionary<string, StockItem> stockItems = await dbContext.LockStockItemsForUpdateAsync(skus, ct);

        foreach (var reservation in activeReservations)
        {
            var sku = reservation.Sku.Trim().ToUpperInvariant();
            var stockItem = stockItems[sku];

            var movement = stockItem.Release(reservation.Quantity, command.OrderId.ToString());
            reservation.Release();

            dbContext.StockMovements.Add(movement);
        }

        List<StockReservationItemDto> releasedItems = [.. activeReservations
            .Select(r => new StockReservationItemDto(r.Sku, r.Quantity))];

        var releasedEvent = new StockReleasedEvent(
            command.OrderId,
            releasedItems,
            command.Reason,
            DateTime.UtcNow);

        await bus.PublishAsync(releasedEvent);

        logger.LogInformation("Stock released for Order {OrderId}. Reason: {Reason}", command.OrderId, command.Reason);

        return Result.Ok();
    }
}

