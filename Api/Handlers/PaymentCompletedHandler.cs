using Api.Data;
using Api.Models;
using Contracts;
using Microsoft.EntityFrameworkCore;

namespace Api.Handlers;

public class PaymentCompletedHandler
{
    public static async Task Handle(
        PaymentCompleted @event,
        OrderDbContext dbContext,
        ILogger<PaymentCompletedHandler> logger,
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
}
