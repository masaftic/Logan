using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wolverine;

namespace Catalog.Api.Features.GetCategoryById;

public static class GetCategoryByIdEndpoint
{
    public static void MapGetCategoryByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories/{id:guid}", async (
            Guid id,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<CategoryDto>>(new QueryCategoryById(id), ct);
            return result.ToHttpResult();
        })
        .WithName("GetCategoryById")
        .WithSummary("Retrieve a category by unique ID")
        .WithTags("Categories")
        .Produces<CategoryDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
