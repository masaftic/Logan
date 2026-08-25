using Contracts.Orders.DTOs;
using Contracts.Orders.Queries;
using Wolverine;

namespace Api.Features.Orders.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void MapGetOrderByIdEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/{id:guid}", async (Guid id, IMessageBus bus) =>
        {
            var response = await bus.InvokeAsync<OrderResponse?>(new GetOrderByIdQuery(id));
            return response is not null 
                ? Results.Ok(response) 
                : Results.NotFound(new { Message = $"Order with ID '{id}' was not found." });
        })
        .WithName("GetOrderById")
        .WithSummary("Fetch order details with payment attempts by ID")
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
