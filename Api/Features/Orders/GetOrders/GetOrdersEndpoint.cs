using Contracts.Orders.DTOs;
using Contracts.Orders.Queries;
using Wolverine;

namespace Api.Features.Orders.GetOrders;

public static class GetOrdersEndpoint
{
    public static void MapGetOrdersEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", async (IMessageBus bus) =>
        {
            var orders = await bus.InvokeAsync<IReadOnlyList<OrderResponse>>(new GetOrdersQuery());
            return Results.Ok(orders);
        })
        .WithName("GetOrders")
        .WithSummary("List all orders with payment history")
        .Produces<IReadOnlyList<OrderResponse>>(StatusCodes.Status200OK);
    }
}
