using Api.Data;
using Contracts.Orders.DTOs;
using Contracts.Orders.Queries;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Orders.GetOrderById;

public static class GetOrderByIdHandler
{
    public static async Task<OrderResponse?> Handle(
        GetOrderByIdQuery query,
        OrderDbContext dbContext,
        CancellationToken ct)
    {
        var order = await dbContext.Orders
            .Include(o => o.Payments)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == query.Id, ct);

        return order?.ToResponse();
    }
}
