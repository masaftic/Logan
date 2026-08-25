using Api.Data;
using Contracts.Orders.DTOs;
using Contracts.Orders.Queries;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Orders.GetOrders;

public static class GetOrdersHandler
{
    public static async Task<IReadOnlyList<OrderResponse>> Handle(
        GetOrdersQuery query,
        OrderDbContext dbContext,
        CancellationToken ct)
    {
        var orders = await dbContext.Orders
            .Include(o => o.Payments)
            .AsNoTracking()
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);

        return orders.Select(o => o.ToResponse()).ToList();
    }
}
