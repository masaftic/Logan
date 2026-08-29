using BuildingBlocks.Common.Results;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Data;
using Ordering.Contracts.Commands;
using Ordering.Contracts.Events;
using Wolverine;

namespace Ordering.Api.Features.CancelOrder;

public static class CancelOrderHandler
{
    public static async Task<Result> Handle(
        CancelOrderCommand command,
        OrderDbContext dbContext,
        IMessageBus bus,
        CancellationToken ct)
    {
        var order = await dbContext.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == command.OrderId, ct);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", $"Order '{command.OrderId}' was not found.");
        }

        var cancelResult = order.Cancel(command.Reason);
        if (cancelResult.IsError)
        {
            return cancelResult.Errors;
        }

        await bus.PublishAsync(new OrderCancelledEvent(
            order.Id,
            command.Reason,
            order.CancelledAtUtc!.Value
        ));

        return Result.Ok();
    }
}
