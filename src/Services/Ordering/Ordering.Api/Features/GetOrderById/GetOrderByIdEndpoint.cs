using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Ordering.Contracts.DTOs;
using Ordering.Contracts.Queries;
using Wolverine;

namespace Ordering.Api.Features.GetOrderById;

public static class GetOrderByIdEndpoint
{
    public static void MapGetOrderByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{orderId:guid}", async (
            [FromRoute] Guid orderId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<OrderDto>>(new GetOrderByIdQuery(orderId), ct);
            return result.ToHttpResult();
        })
        .WithName("GetOrderById")
        .WithSummary("Get order by ID")
        .WithTags("Orders")
        .Produces<OrderDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
