using BuildingBlocks.Common.Results;
using Inventory.Api.Data;
using Inventory.Api.Domain.Errors;
using Inventory.Contracts.DTOs;
using Inventory.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Features.GetStock;

public class GetStockHandler
{
    public async Task<Result<StockItemDto>> Handle(
        GetStockBySkuQuery query,
        InventoryDbContext dbContext,
        CancellationToken ct)
    {
        var normalizedSku = query.Sku.Trim().ToUpperInvariant();

        var stockItem = await dbContext.StockItems
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Sku == normalizedSku, ct);

        if (stockItem is null)
        {
            return InventoryErrors.StockNotFound(normalizedSku);
        }

        var dto = new StockItemDto(
            stockItem.Sku,
            stockItem.Name,
            stockItem.QuantityAvailable,
            stockItem.QuantityReserved,
            stockItem.QuantityOnHand
        );

        return dto;
    }
}

