using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Inventory.Contracts.Commands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Inventory.Api.Features.RestockItem;

public static class RestockItemEndpoint
{
    public static void MapRestockItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/inventory/restock", async (
            [FromBody] RestockItemCommand command,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToAcceptedResult();
        })
        .WithName("RestockItem")
        .WithSummary("Restock an inventory item")
        .WithTags("Inventory");
    }
}

