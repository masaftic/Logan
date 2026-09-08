using BuildingBlocks.Common.Results;
using Catalog.Api.Data;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Features.GetProductsBySkus;

public static class GetProductsBySkusHandler
{
    public static async Task<Result<IReadOnlyList<ProductDto>>> Handle(
        QueryProductsBySkus query,
        CatalogDbContext dbContext,
        CancellationToken ct)
    {
        if (query.Skus is null || query.Skus.Count == 0)
        {
            return Result<IReadOnlyList<ProductDto>>.Ok([]);
        }

        var normalizedSkus = query.Skus
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        if (normalizedSkus.Length == 0)
        {
            return Result<IReadOnlyList<ProductDto>>.Ok([]);
        }

        var products = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.IsActive && normalizedSkus.Contains((string)p.Sku))
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
