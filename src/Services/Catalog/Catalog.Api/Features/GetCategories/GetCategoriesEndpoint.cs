using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Catalog.Api.Features.GetCategories;

public static class GetCategoriesEndpoint
{
    public static void MapGetCategoriesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<IReadOnlyList<CategoryDto>>>(new QueryCategories(), ct);
            return result.ToHttpResult();
        })
        .WithName("GetCategories")
        .WithSummary("Retrieve all product categories")
        .WithTags("Categories")
        .Produces<IReadOnlyList<CategoryDto>>(StatusCodes.Status200OK);
    }
}
