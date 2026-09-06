using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shipping.Contracts.Commands;
using Shipping.Contracts.DTOs;
using Wolverine;

namespace Shipping.Api.Features.CreateShipment;

public static class CreateShipmentEndpoint
{
    public static void MapCreateShipmentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/shipping/shipments", async (
            [FromBody] CommandCreateShipment command,
            [FromServices] IMessageBus bus,
            CancellationToken cancellationToken) =>
        {
            var result = await bus.InvokeAsync<Result<ShipmentDto>>(command, cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("CreateShipment")
        .WithSummary("Create shipment and purchase carrier label with Shippo based on selected rate")
        .WithTags("Shipping")
        .Produces<ShipmentDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
