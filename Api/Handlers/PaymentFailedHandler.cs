using Api.Data;
using Api.Models;
using Contracts;
using Microsoft.EntityFrameworkCore;

namespace Api.Handlers;

public class PaymentFailedHandler
{
    public static async Task Handle(
        PaymentFailed @event,
        OrderDbContext dbContext,
        ILogger<PaymentFailedHandler> logger,
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
