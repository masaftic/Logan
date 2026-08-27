using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Inventory.Contracts.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Inventory.Api.Features.ReserveStock;

public static class ReserveStockEndpoint
{
    public static void MapReserveStockEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/inventory/reservations", async (
            [FromBody] ReserveStockCommand command,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToAcceptedResult();
        })
        .WithName("ReserveStock")
        .WithSummary("Reserve stock for an order")
        .WithTags("Inventory");
    }
}
