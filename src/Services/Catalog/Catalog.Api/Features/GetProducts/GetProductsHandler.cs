using BuildingBlocks.Common.Results;
using Catalog.Api.Data;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Features.GetProducts;

public static class GetProductsHandler
{
    public static async Task<Result<IReadOnlyList<ProductDto>>> Handle(
        QueryProducts query,
        CatalogDbContext dbContext,
        CancellationToken ct)
    {
        var dbQuery = dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var pageNumber = Math.Max(query.PageNumber, 1);

        var products = await dbQuery
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return products.Select(p => new ProductDto(
            p.Id,
            p.Sku,
            p.Name,
            p.Description,
            p.Price,
            p.Currency,
            p.Weight.Value,
            p.Weight.Unit.Key,
            p.Dimensions.Length.Value,
            p.Dimensions.Width.Value,
            p.Dimensions.Height.Value,
            p.Dimensions.Length.Unit.Key,
            p.CategoryId,
            p.Category.Name)).ToList();
    }
}
