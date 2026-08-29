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

public class ReserveStockHandler
{
    public static async Task<Result> Handle(
        ReserveStockCommand command,
        InventoryDbContext dbContext,
        IMessageBus bus,
        ILogger<ReserveStockHandler> logger,
        CancellationToken ct)
    {
        List<StockReservation> existingReservations = await dbContext.StockReservations
            .WhereActiveForOrder(command.OrderId)
            .ToListAsync(ct);

        // Idempotent
        if (existingReservations.Count > 0)
        {
            return Result.Ok();
        }

        List<StockReservationItemDto> normalizedItems = [.. command.Items];
        List<Sku> skus = [.. normalizedItems.Select(i => Sku.Create(i.Sku))];

        Dictionary<Sku, StockItem> stockItems = await dbContext.LockStockItemsForUpdateAsync(skus, ct);

        foreach (var item in normalizedItems)
        {
            var sku = Sku.Create(item.Sku);
            var requestedQty = PositiveQuantity.Create(item.Quantity);

            if (!stockItems.TryGetValue(sku, out var stockItem) || !stockItem.CanReserve(requestedQty))
            {
                var available = stockItem is not null ? stockItem.QuantityAvailable : 0;
                var exhaustedEvent = new StockExhaustedEvent(command.OrderId, sku, item.Quantity, available);

                await bus.PublishAsync(exhaustedEvent);
                return InventoryErrors.InsufficientStock(sku, item.Quantity, available);
            }
        }

        var holdDuration = command.HoldDurationMinutes.HasValue
            ? TimeSpan.FromMinutes(command.HoldDurationMinutes.Value)
            : StockReservation.DefaultHoldDuration;

        foreach (var item in normalizedItems)
        {
            var sku = Sku.Create(item.Sku);
            var requestedQty = PositiveQuantity.Create(item.Quantity);
            var stockItem = stockItems[sku];

            var movementResult = stockItem.Reserve(requestedQty, command.OrderId.ToString());
            var reservationResult = StockReservation.Create(command.OrderId, sku, requestedQty, holdDuration);

            dbContext.StockMovements.Add(movementResult.Value);
            dbContext.StockReservations.Add(reservationResult.Value);
        }

        var reservedEvent = new StockReservedEvent(command.OrderId, command.Items, DateTime.UtcNow);
        await bus.PublishAsync(reservedEvent);

        await bus.ScheduleAsync(
            new ReleaseStockCommand(command.OrderId, "Reservation TTL Expired"), 
            holdDuration);

        logger.LogInformation("Stock reserved for Order {OrderId}.", command.OrderId);

        return Result.Ok();
    }
}
