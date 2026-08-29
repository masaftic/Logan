using BuildingBlocks.Common.ValueObjects;
using Inventory.Api.Domain;
using Inventory.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data.Extensions;

public static class InventoryDbContextExtensions
{
    public static async Task<Dictionary<Sku, StockItem>> LockStockItemsForUpdateAsync(
        this InventoryDbContext dbContext,
        IReadOnlyList<Sku> skus,
        CancellationToken ct = default)
    {
        if (skus.Count == 0)
        {
            return [];
        }

        string[] sortedSkus = skus
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .Select(s => (string)s)
            .ToArray();

        var stockItems = await dbContext.StockItems
            .FromSqlInterpolated($"SELECT *, xmin FROM inventory.stock_items WHERE sku = ANY({sortedSkus}) ORDER BY sku FOR UPDATE")
            .ToDictionaryAsync(s => s.Sku, ct);

        return stockItems;
    }

    public static IQueryable<StockReservation> WhereActiveForOrder(this IQueryable<StockReservation> reservations, Guid orderId)
        => reservations.Where(r => r.OrderId == orderId && r.Status == ReservationStatus.Active);
}
