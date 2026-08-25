using Contracts.Payments.Commands;
using Contracts.Payments.DTOs;
using Wolverine;

namespace Api.Features.Payments.CreatePaymentIntent;

public static class CreatePaymentIntentEndpoint
{
    public static void MapCreatePaymentIntentEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/orders/{id:guid}/pay", async (Guid id, IMessageBus bus) =>
        {
            try
            {
                var response = await bus.InvokeAsync<PaymentIntentResponse>(new CreatePaymentIntentCommand(id));
                return Results.Ok(response);
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
        .WithName("CreatePaymentIntent")
        .WithSummary("Initialize a PaymentIntent for an order")
        .Produces<PaymentIntentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
