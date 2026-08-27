using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Inventory.Contracts.DTOs;
using Inventory.Contracts.Queries;
using Wolverine;

namespace Inventory.Api.Features.GetStockHistory;

public static class GetStockHistoryEndpoint
{
    public static void MapGetStockHistoryEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/inventory/{sku}/history", async (
            [AsParameters] GetStockHistoryQuery query,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<IReadOnlyList<StockMovementDto>>>(query, ct);
            return result.ToHttpResult();
        })
        .Produces<IReadOnlyList<StockMovementDto>>(StatusCodes.Status200OK)
        .WithName("GetStockHistory")
        .WithSummary("Retrieve stock movement audit history for a specific SKU")
        .WithTags("Inventory");
    }
}

