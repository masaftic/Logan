using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Ordering.Contracts.Commands;
using Wolverine;

namespace Ordering.Api.Features.CancelOrder;

public static class CancelOrderEndpoint
{
    public static void MapCancelOrderEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders/{id:guid}/cancel", async (
            Guid id,
            [FromBody] CancelOrderCommand command,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            command = command with { OrderId = id };
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToHttpResult();
        })
        .WithName("CancelOrder")
        .WithSummary("Cancel an order")
        .WithTags("Orders")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
