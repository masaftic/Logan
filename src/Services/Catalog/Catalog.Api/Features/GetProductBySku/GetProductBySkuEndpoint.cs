using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Catalog.Api.Features.GetProductBySku;

public static class GetProductBySkuEndpoint
{
    public static void MapGetProductBySkuEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/sku/{sku}", async (
            string sku,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ProductDto>>(new QueryProductBySku(sku), ct);
            return result.ToHttpResult();
        })
        .WithName("GetProductBySku")
        .WithSummary("Retrieve a product by SKU")
        .WithTags("Products")
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
