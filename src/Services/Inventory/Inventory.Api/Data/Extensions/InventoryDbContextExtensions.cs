using Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data.Extensions;

public static class InventoryDbContextExtensions
{
    public static async Task<Dictionary<string, StockItem>> LockStockItemsForUpdateAsync(
        this InventoryDbContext dbContext,
        IReadOnlyList<string> sortedSkus,
        CancellationToken ct = default)
    {
        if (sortedSkus.Count == 0)
        {
            return [];
        }

        var skuArray = sortedSkus.ToArray();

        var stockItems = await dbContext.StockItems
            .FromSqlInterpolated($"SELECT *, xmin FROM inventory.stock_items WHERE sku = ANY({skuArray}) ORDER BY sku FOR UPDATE")
            .ToDictionaryAsync(s => s.Sku, ct);

        return stockItems;
    }
}
