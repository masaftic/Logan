using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Catalog.Api.Features.GetProductsBySkus;

public static class GetProductsBySkusEndpoint
{
    public static void MapGetProductsBySkusEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/products/by-skus", async (
            [FromBody] QueryProductsBySkus query,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<IReadOnlyList<ProductDto>>>(query, ct);
            return result.ToHttpResult();
        })
        .WithName("GetProductsBySkusPost")
        .WithSummary("Retrieve products by a batch of SKUs via JSON body")
        .WithTags("Products")
        .Produces<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK);

        app.MapGet("/api/products/by-skus", async (
            [FromQuery] string[] skus,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new QueryProductsBySkus(skus);
            var result = await bus.InvokeAsync<Result<IReadOnlyList<ProductDto>>>(query, ct);
            return result.ToHttpResult();
        })
        .WithName("GetProductsBySkusGet")
        .WithSummary("Retrieve products by a list of SKUs via query string")
        .WithTags("Products")
        .Produces<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK);
    }
}
