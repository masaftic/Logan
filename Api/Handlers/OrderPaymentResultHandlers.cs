using Api.Data;
using Api.Models;
using Contracts;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Handlers;

public class OrderPaymentResultHandlers
{
    public static async Task Handle(
        PaymentCompleted @event,
        OrderDbContext dbContext,
        ILogger<OrderPaymentResultHandlers> logger,
        CancellationToken ct)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found when applying PaymentCompleted.", @event.OrderId);
            return;
        }

        order.Status = OrderStatus.Completed;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} status updated to Completed (Payment ID: {PaymentId}).",
            order.Id, @event.PaymentId);
    }

    public static async Task Handle(
        PaymentFailed @event,
        OrderDbContext dbContext,
        ILogger<OrderPaymentResultHandlers> logger,
        CancellationToken ct)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found when applying PaymentFailed.", @event.OrderId);
            return;
        }

        order.Status = OrderStatus.Failed;
        order.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(ct);

        logger.LogWarning("Order {OrderId} status updated to Failed. Reason: {Reason}",
            order.Id, @event.Reason);
    }
}
