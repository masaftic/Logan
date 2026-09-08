using BuildingBlocks.Common.Results;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Data;
using Ordering.Contracts.DTOs;
using Ordering.Contracts.Enums;
using Ordering.Contracts.Queries;

namespace Ordering.Api.Features.GetOrderSummary;

public static class GetOrderSummaryHandler
{
    public static async Task<Result<OrderSummaryDto>> Handle(
        GetOrderSummaryQuery query,
        OrderDbContext dbContext,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.OrderId == query.OrderId, ct);

        if (summary is null)
        {
            return Error.NotFound("OrderSummary.NotFound", $"Order summary for order '{query.OrderId}' was not found.");
        }

        return new OrderSummaryDto(
            summary.OrderId,
            summary.CustomerId,
            summary.TotalAmount,
            summary.TotalItemsCount,
            (OrderStatusDto)summary.OrderStatus,
            (InventoryStatusDto)summary.InventoryStatus,
            (PaymentStatusDto)summary.PaymentStatus,
            (ShippingStatusDto)summary.ShippingStatus,
            (CustomerOrderStatusDto)summary.CustomerStatus,
            summary.CreatedAtUtc,
            summary.LastModifiedAtUtc);
    }
}
