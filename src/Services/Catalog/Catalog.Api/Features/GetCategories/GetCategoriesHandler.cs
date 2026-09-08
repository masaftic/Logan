using BuildingBlocks.Common.Results;
using Catalog.Api.Data;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Features.GetCategories;

public static class GetCategoriesHandler
{
    public static async Task<Result<IReadOnlyList<CategoryDto>>> Handle(
        QueryCategories query,
        CatalogDbContext dbContext,
        CancellationToken ct)
    {
        List<CategoryDto> categories = await dbContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Slug,
                c.Description))
            .ToListAsync(ct);

        return categories;
    }
}
