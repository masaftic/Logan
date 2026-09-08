using BuildingBlocks.Common.ValueObjects;
using Inventory.Contracts.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Api.Data;
using Ordering.Api.Domain.Enums;
using Ordering.Api.Domain.ReadModels;
using Ordering.Contracts.Events;
using Payment.Contracts.Events;
using Shipping.Contracts.Events;

namespace Ordering.Api.Features.GetOrderSummary;

public class OrderSummaryProjectionHandlers
{
    public static async Task Handle(
        OrderSubmittedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is null)
        {
            summary = OrderSummary.Create(
                @event.OrderId,
                @event.CustomerId,
                Price.Create(@event.TotalAmount),
                @event.Items.Count,
                @event.CreatedAtUtc);
            summary.UpdateInventoryStatus(InventoryStatus.Reserved);
            dbContext.OrderSummaries.Add(summary);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary created for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        StockReservedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdateInventoryStatus(InventoryStatus.Reserved);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to StockReserved for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        StockDeductedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdateInventoryStatus(InventoryStatus.Confirmed);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to StockConfirmed for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        StockReleasedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdateInventoryStatus(InventoryStatus.Released);
            summary.MarkCancelled(@event.ReleasedAtUtc);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to StockReleased and Cancelled for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        PaymentCompletedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdatePaymentStatus(PaymentStatus.Captured);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to PaymentCaptured for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        PaymentFailedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdatePaymentStatus(PaymentStatus.Failed);
            summary.MarkCancelled(@event.FailedAtUtc);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to PaymentFailed and Cancelled for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        PaymentRefundedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdatePaymentStatus(PaymentStatus.Refunded);
            summary.MarkCancelled(@event.RefundedAtUtc);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to PaymentRefunded and Cancelled for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        ShipmentLabelPurchasedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdateShippingStatus(ShippingStatus.Dispatched);
            summary.MarkCompleted(@event.DispatchedAtUtc);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to ShipmentDispatched and Completed for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        ShipmentCreationFailedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdateShippingStatus(ShippingStatus.Failed);
            summary.MarkCancelled(@event.FailedAtUtc);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to ShipmentFailed and Cancelled for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        ShipmentInTransitEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdateShippingStatus(ShippingStatus.Dispatched);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to ShipmentInTransit for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        ShipmentDeliveredEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdateShippingStatus(ShippingStatus.Dispatched);
            summary.MarkCompleted(@event.DeliveredAtUtc);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to ShipmentDelivered and Completed for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        ShipmentDeliveryFailedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.UpdateShippingStatus(ShippingStatus.Failed);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to ShipmentDeliveryFailed for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        OrderCancelledEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.MarkCancelled(@event.CancelledAtUtc);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to Cancelled for Order {OrderId}.", @event.OrderId);
        }
    }

    public static async Task Handle(
        OrderCompletedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSummaryProjectionHandlers> logger,
        CancellationToken ct)
    {
        var summary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == @event.OrderId, ct);
        if (summary is not null)
        {
            summary.MarkCompleted(@event.CompletedAtUtc);
            await dbContext.SaveChangesAsync(ct);
            logger.LogInformation("OrderSummary updated to Completed for Order {OrderId}.", @event.OrderId);
        }
    }
}
