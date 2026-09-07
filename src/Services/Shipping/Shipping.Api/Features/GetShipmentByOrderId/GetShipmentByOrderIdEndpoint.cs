using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Queries;
using Wolverine;

namespace Shipping.Api.Features.GetShipmentByOrderId;

public static class GetShipmentByOrderIdEndpoint
{
    public static void MapGetShipmentByOrderIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/shipping/shipments/order/{orderId:guid}", async (
            [FromRoute] Guid orderId,
            [FromServices] IMessageBus bus,
            CancellationToken cancellationToken) =>
        {
            var result = await bus.InvokeAsync<Result<ShipmentDetailsDto>>(
                new QueryGetShipmentByOrderId(orderId),
                cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("GetShipmentByOrderId")
        .WithSummary("Get shipment details and tracking history by order ID")
        .WithTags("Shipping")
        .Produces<ShipmentDetailsDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
