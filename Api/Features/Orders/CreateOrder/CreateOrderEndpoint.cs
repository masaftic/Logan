using Contracts.Orders.Commands;
using Contracts.Orders.DTOs;
using Wolverine;

namespace Api.Features.Orders.CreateOrder;

public static class CreateOrderEndpoint
{
    public static void MapCreateOrderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", async (CreateOrderCommand command, IMessageBus bus) =>
        {
            var response = await bus.InvokeAsync<OrderResponse>(command);
            return Results.Created($"/api/orders/{response.Id}", response);
        })
        .WithName("CreateOrder")
        .WithSummary("Place a new order")
        .Produces<OrderResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}
