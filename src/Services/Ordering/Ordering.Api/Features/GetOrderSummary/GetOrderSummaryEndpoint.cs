using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Ordering.Contracts.DTOs;
using Ordering.Contracts.Queries;
using Wolverine;

namespace Ordering.Api.Features.GetOrderSummary;

public static class GetOrderSummaryEndpoint
{
    public static void MapGetOrderSummaryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders/{orderId:guid}/summary", async (
            [FromRoute] Guid orderId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<OrderSummaryDto>>(new GetOrderSummaryQuery(orderId), ct);
            return result.ToHttpResult();
        })
        .WithName("GetOrderSummary")
        .WithSummary("Get order dimensional summary and customer status")
        .WithTags("Orders")
        .Produces<OrderSummaryDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
