using Contracts.Orders.DTOs;
using Wolverine;

namespace Api.Features.Orders.CreateOrder;

public static class CreateOrderEndpoint
{
    public static void MapCreateOrderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/", async (CreateOrderRequest request, IMessageBus bus) =>
        {
            var response = await bus.InvokeAsync<OrderResponse>(new Contracts.Orders.Commands.CreateOrderCommand(request.Amount));
            return Results.Created($"/api/orders/{response.Id}", response);
        })
        .WithName("CreateOrder")
        .WithSummary("Place a new order")
        .Produces<OrderResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();
    }
}
