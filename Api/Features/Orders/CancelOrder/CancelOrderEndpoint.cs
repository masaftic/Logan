using Contracts.Orders.Commands;
using Wolverine;

namespace Api.Features.Orders.CancelOrder;

public static class CancelOrderEndpoint
{
    public static void MapCancelOrderEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/{id:guid}/cancel", async (Guid id, IMessageBus bus) =>
        {
            try
            {
                var found = await bus.InvokeAsync<bool>(new CancelOrderCommand(id));
                return found 
                    ? Results.Ok(new { Message = "Order cancelled successfully." }) 
                    : Results.NotFound(new { Message = $"Order with ID '{id}' was not found." });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Message = ex.Message });
            }
        })
        .WithName("CancelOrder")
        .WithSummary("Cancel an order")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
