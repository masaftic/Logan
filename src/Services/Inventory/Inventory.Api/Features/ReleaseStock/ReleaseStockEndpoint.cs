using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Inventory.Contracts.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Inventory.Api.Features.ReleaseStock;

public static class ReleaseStockEndpoint
{
    public static void MapReleaseStockEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/inventory/reservations/release", async (
            [FromBody] ReleaseStockCommand command,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToAcceptedResult();
        })
        .WithName("ReleaseStock")
        .WithSummary("Release stock reserved for an order")
        .WithTags("Inventory");
    }
}

