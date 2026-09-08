using BuildingBlocks.Common.Results;
using Catalog.Api.Data;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Features.GetCategoryById;

public static class GetCategoryByIdHandler
{
    public static async Task<Result<CategoryDto>> Handle(
        QueryCategoryById query,
        CatalogDbContext dbContext,
        CancellationToken ct)
    {
        var category = await dbContext.Categories
            .AsNoTracking()
            .Where(c => c.Id == query.Id)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description))
            .SingleOrDefaultAsync(ct);

        if (category is null)
        {
            return Error.NotFound("Category.NotFound", $"Category with ID '{query.Id}' was not found.");
        }

        return category;
    }
}
