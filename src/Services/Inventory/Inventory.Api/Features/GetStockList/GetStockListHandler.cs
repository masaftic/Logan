using BuildingBlocks.Common.Results;
using Inventory.Api.Data;
using Inventory.Contracts.DTOs;
using Inventory.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Features.GetStockList;

public static class GetStockListHandler
{
    public static async Task<Result<IReadOnlyList<StockItemDto>>> Handle(
        GetStockListQuery query,
        InventoryDbContext dbContext,
        CancellationToken ct)
    {
        var dbQuery = dbContext.StockItems.AsNoTracking();

        if (query.Skus is { Count: > 0 })
        {
            var normalizedSkus = query.Skus.Select(s => s.Trim().ToUpperInvariant()).ToArray();
            dbQuery = dbQuery.Where(x => normalizedSkus.Contains(x.Sku));
        }

        List<StockItemDto> items = await dbQuery
            .OrderBy(x => x.Sku)
            .Select(x => new StockItemDto(
                x.Sku,
                x.Name,
                x.QuantityAvailable,
                x.QuantityReserved,
                x.QuantityOnHand
            ))
            .ToListAsync(ct);

        return items;
    }
}

