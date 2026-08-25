using Contracts.Payments.Commands;
using Contracts.Payments.DTOs;
using Wolverine;

namespace Api.Features.Payments.MarkPaymentPaid;

public static class MarkPaymentPaidEndpoint
{
    public static void MapMarkPaymentPaidEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/orders/{id:guid}/mark-paid", async (
            Guid id,
            IMessageBus bus) =>
        {
            try
            {
                var command = new MarkPaymentPaidCommand(OrderId: id);

                await bus.InvokeAsync(command);

                return Results.Ok(new { Message = $"Payment for Order '{id}' marked as Paid." });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { Message = ex.Message });
            }
        })
        .WithName("MarkPaymentPaid")
        .WithSummary("Admin/Manual endpoint to mark an order payment as paid")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
