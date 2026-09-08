using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Catalog.Api.Data;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Features.GetProductBySku;

public static class GetProductBySkuHandler
{
    public static async Task<Result<ProductDto>> Handle(
        QueryProductBySku query,
        CatalogDbContext dbContext,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(query.Sku))
        {
            return Error.Validation("Product.InvalidSku", "SKU cannot be empty.");
        }

        var sku = Sku.Create(query.Sku);

        var product = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .SingleOrDefaultAsync(p => p.Sku == sku && p.IsActive, ct);

        if (product is null)
        {
            return Error.NotFound("Product.NotFound", $"Product with SKU '{query.Sku}' was not found.");
        }

        return new ProductDto(
            product.Id,
            product.Sku,
            product.Name,
            product.Description,
            product.Price,
            product.Currency,
            product.Weight.Value,
            product.Weight.Unit.Key,
            product.Dimensions.Length.Value,
            product.Dimensions.Width.Value,
            product.Dimensions.Height.Value,
            product.Dimensions.Length.Unit.Key,
            product.CategoryId,
            product.Category.Name);
    }
}
