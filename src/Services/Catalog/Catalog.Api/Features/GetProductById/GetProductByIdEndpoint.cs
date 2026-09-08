using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Catalog.Api.Features.GetProductById;

public static class GetProductByIdEndpoint
{
    public static void MapGetProductByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", async (
            Guid id,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<ProductDto>>(new QueryProductById(id), ct);
            return result.ToHttpResult();
        })
        .WithName("GetProductById")
        .WithSummary("Retrieve a product by unique ID")
        .WithTags("Products")
        .Produces<ProductDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
