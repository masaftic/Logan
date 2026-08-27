using BuildingBlocks.Common.Results;
using Inventory.Api.Data;
using Inventory.Api.Domain.Errors;
using Inventory.Contracts.DTOs;
using Inventory.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Features.GetStockHistory;

public static class GetStockHistoryHandler
{
    public static async Task<Result<IReadOnlyList<StockMovementDto>>> Handle(
        GetStockHistoryQuery query,
        InventoryDbContext dbContext,
        CancellationToken ct)
    {
        var normalizedSku = query.Sku.Trim().ToUpperInvariant();

        var itemExists = await dbContext.StockItems
            .AsNoTracking()
            .AnyAsync(x => x.Sku == normalizedSku, ct);

        if (!itemExists)
        {
            return InventoryErrors.StockNotFound(normalizedSku);
        }

        var limit = Math.Clamp(query.Limit, 1, 200);

        var movements = await dbContext.StockMovements
            .AsNoTracking()
            .Where(x => x.Sku == normalizedSku)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(limit)
            .Select(x => new
            {
                x.Id,
                x.Sku,
                x.QuantityDelta,
                x.AvailableAfter,
                x.ReservedAfter,
                x.Type,
                x.ReferenceId,
                x.CreatedAtUtc
            })
            .ToListAsync(ct);

        return movements.Select(x => new StockMovementDto(
            x.Id,
            x.Sku,
            x.QuantityDelta,
            x.AvailableAfter,
            x.ReservedAfter,
            (Inventory.Contracts.DTOs.StockMovementType)x.Type,
            x.ReferenceId,
            x.CreatedAtUtc
        )).ToList();
    }
}

