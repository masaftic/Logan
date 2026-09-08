using BuildingBlocks.Common.Results;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Data;
using Ordering.Contracts.DTOs;
using Ordering.Contracts.Enums;
using Ordering.Contracts.Queries;

namespace Ordering.Api.Features.GetOrderById;

public static class GetOrderByIdHandler
{
    public static async Task<Result<OrderDto>> Handle(
        GetOrderByIdQuery query,
        OrderDbContext dbContext,
        CancellationToken ct)
    {
        var order = await dbContext.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == query.OrderId, ct);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", $"Order with ID '{query.OrderId}' was not found.");
        }

        List<OrderItemDto> items = [.. order.Items.Select(i => new OrderItemDto(
            i.Sku,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice))];

        return new OrderDto(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            (OrderStatusDto)order.Status,
            items,
            order.CreatedAtUtc,
            order.CompletedAtUtc,
            order.CancelledAtUtc,
            order.CancellationReason);
    }
}
