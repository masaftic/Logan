using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Catalog.Api.Features.GetProducts;

public static class GetProductsEndpoint
{
    public static void MapGetProductsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async (
            [FromQuery] Guid? categoryId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            IMessageBus bus = null!,
            CancellationToken ct = default) =>
        {
            var query = new QueryProducts(categoryId, pageNumber, pageSize);
            var result = await bus.InvokeAsync<Result<IReadOnlyList<ProductDto>>>(query, ct);
            return result.ToHttpResult();
        })
        .WithName("GetProducts")
        .WithSummary("Retrieve products with optional category filtering and pagination")
        .WithTags("Products")
        .Produces<IReadOnlyList<ProductDto>>(StatusCodes.Status200OK);
    }
}
