using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Inventory.Api.Data;
using Inventory.Api.Data.Extensions;
using Inventory.Api.Domain;
using Inventory.Api.Domain.Enums;
using Inventory.Api.Domain.Errors;
using Inventory.Contracts.Commands;
using Inventory.Contracts.DTOs;
using Inventory.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Inventory.Api.Features.ReserveStock;

public static class ReserveStockHandler
{
    public static async Task<Result> Handle(
        ReserveStockCommand command,
        InventoryDbContext dbContext,
        IMessageBus bus,
        CancellationToken ct)
    {
        List<StockReservation> existingReservations = await dbContext.StockReservations
            .Where(r => r.OrderId == command.OrderId && r.Status == ReservationStatus.Active)
            .ToListAsync(ct);

        // Idempotent
        if (existingReservations.Count > 0)
        {
            return Result.Ok();
        }

        List<StockReservationItemDto> normalizedItems = [.. command.Items.OrderBy(i => i.Sku, StringComparer.OrdinalIgnoreCase)];
        List<string> skus = [.. normalizedItems.Select(i => i.Sku.Trim().ToUpperInvariant())];

        Dictionary<string, StockItem> stockItems = await dbContext.LockStockItemsForUpdateAsync(skus, ct);

        foreach (var item in normalizedItems)
        {
            var sku = item.Sku.Trim().ToUpperInvariant();
            var requestedQty = PositiveQuantity.Create(item.Quantity);

            if (!stockItems.TryGetValue(sku, out var stockItem) || !stockItem.CanReserve(requestedQty))
            {
                var available = stockItem is not null ? stockItem.QuantityAvailable : 0;
                var exhaustedEvent = new StockExhaustedEvent(command.OrderId, sku, item.Quantity, available);

                await bus.PublishAsync(exhaustedEvent);
                return InventoryErrors.InsufficientStock(sku, item.Quantity, available);
            }
        }

        foreach (var item in normalizedItems)
        {
            var sku = item.Sku.Trim().ToUpperInvariant();
            var requestedQty = PositiveQuantity.Create(item.Quantity);
            var stockItem = stockItems[sku];

            var movementResult = stockItem.Reserve(requestedQty, command.OrderId.ToString());
            var reservationResult = StockReservation.Create(command.OrderId, sku, requestedQty);

            dbContext.StockMovements.Add(movementResult.Value);
            dbContext.StockReservations.Add(reservationResult.Value);
        }

        var reservedEvent = new StockReservedEvent(command.OrderId, command.Items, DateTime.UtcNow);
        await bus.PublishAsync(reservedEvent);

        return Result.Ok();
    }
}
