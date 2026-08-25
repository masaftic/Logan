using Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Orders.CancelOrder;

public static class CancelOrderHandler
{
    public static async Task<bool> Handle(
        Contracts.Orders.Commands.CancelOrderCommand command,
        OrderDbContext dbContext,
        CancellationToken ct)
    {
        var order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

        if (order is null)
            return false;

        order.Cancel();
        return true;
    }
}
