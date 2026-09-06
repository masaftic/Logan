using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Queries;
using Wolverine;

namespace Shipping.Api.Features.EstimateShippingRates;

public static class EstimateShippingRatesEndpoint
{
    public static void MapEstimateShippingRatesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/shipping/rates/estimate", async (
            [FromBody] QueryShippingRates query,
            [FromServices] IMessageBus bus,
            CancellationToken cancellationToken) =>
        {
            var result = await bus.InvokeAsync<Result<IReadOnlyList<ShippingRateQuoteDto>>>(query, cancellationToken);

            return result.ToHttpResult();
        })
        .WithName("EstimateShippingRates")
        .WithSummary("Estimate shipping rates")
        .WithTags("Shipping")
        .Produces<IReadOnlyList<ShippingRateQuoteDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
