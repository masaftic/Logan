using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Inventory.Contracts.DTOs;
using Inventory.Contracts.Queries;
using Wolverine;

namespace Inventory.Api.Features.GetStock;

public static class GetStockEndpoint
{
    public static void MapGetStockEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inventory/{sku}", async (
            [AsParameters] GetStockBySkuQuery query,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<StockItemDto>>(query, ct);
            return result.ToHttpResult();
        })
        .Produces<StockItemDto>(StatusCodes.Status200OK)
        .WithName("GetStockBySku")
        .WithSummary("Retrieve current stock levels for a specific SKU")
        .WithTags("Inventory");
    }
}

