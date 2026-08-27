using BuildingBlocks.Common.Results;
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

namespace Inventory.Api.Features.ConfirmStockDeduction;

public static class ConfirmStockDeductionHandler
{
    public static async Task<Result> Handle(
        ConfirmStockDeductionCommand command,
        InventoryDbContext dbContext,
        IMessageBus bus,
        CancellationToken ct)
    {
        List<StockReservation> activeReservations = await dbContext.StockReservations
            .WhereActiveForOrder(command.OrderId)
            .ToListAsync(ct);

        if (activeReservations.Count == 0)
        {
            var alreadyConfirmed = await dbContext.StockReservations
                .AnyAsync(r => r.OrderId == command.OrderId && r.Status == ReservationStatus.Confirmed, ct);

            if (alreadyConfirmed)
            {
                return Result.Ok();
            }

            return InventoryErrors.ReservationNotFound(command.OrderId);
        }

        List<string> skus = [.. activeReservations
            .Select(r => r.Sku.Trim().ToUpperInvariant())
            .Distinct() ];

        Dictionary<string, StockItem> stockItems = await dbContext.LockStockItemsForUpdateAsync(skus, ct);

        foreach (var reservation in activeReservations)
        {
            var sku = reservation.Sku.Trim().ToUpperInvariant();
            var stockItem = stockItems[sku];

            var movement = stockItem.ConfirmDeduction(reservation.Quantity, command.OrderId.ToString());
            reservation.Confirm();

            dbContext.StockMovements.Add(movement);
        }

        List<StockReservationItemDto> deductedItems = [.. activeReservations
            .Select(r => new StockReservationItemDto(r.Sku, (int)r.Quantity))];

        var deductedEvent = new StockDeductedEvent(
            command.OrderId,
            deductedItems,
            DateTime.UtcNow);

        await bus.PublishAsync(deductedEvent);

        return Result.Ok();
    }
}

