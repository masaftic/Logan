using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Queries;
using Wolverine;

namespace Shipping.Api.Features.GetShipmentById;

public static class GetShipmentByIdEndpoint
{
    public static void MapGetShipmentByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/shipping/shipments/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IMessageBus bus,
            CancellationToken cancellationToken) =>
        {
            var result = await bus.InvokeAsync<Result<ShipmentDetailsDto>>(
                new QueryGetShipmentById(id),
                cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("GetShipmentById")
        .WithSummary("Get shipment details and tracking history by shipment ID")
        .WithTags("Shipping")
        .Produces<ShipmentDetailsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
