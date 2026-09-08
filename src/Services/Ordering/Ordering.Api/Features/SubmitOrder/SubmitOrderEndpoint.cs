using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Ordering.Contracts.Commands;
using Ordering.Contracts.DTOs;
using Wolverine;

namespace Ordering.Api.Features.SubmitOrder;

public static class SubmitOrderEndpoint
{
    public static void MapSubmitOrderEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (
            [FromBody] SubmitOrderCommand command,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<OrderDto>>(command, ct);
            return result.ToCreatedResult(res => $"/api/orders/{res.Id}");
        })
        .WithName("SubmitOrder")
        .WithSummary("Submit a new order")
        .WithTags("Orders")
        .Produces<OrderDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
