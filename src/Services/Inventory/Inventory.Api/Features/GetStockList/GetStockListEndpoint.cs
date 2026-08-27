using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Inventory.Contracts.DTOs;
using Inventory.Contracts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Inventory.Api.Features.GetStockList;

public static class GetStockListEndpoint
{
    public static void MapGetStockListEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inventory", async (
            string[] skus,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new GetStockListQuery(skus);
            var result = await bus.InvokeAsync<Result<IReadOnlyList<StockItemDto>>>(query, ct);
            return result.ToHttpResult();
        })
        .Produces<IReadOnlyList<StockItemDto>>(StatusCodes.Status200OK)
        .WithName("GetStockList")
        .WithSummary("Retrieve stock levels for all or specified SKUs")
        .WithTags("Inventory");
    }
}

