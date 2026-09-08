using BuildingBlocks.Common.Results;
using Catalog.Api.Data;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Features.GetProductById;

public static class GetProductByIdHandler
{
    public static async Task<Result<ProductDto>> Handle(
        QueryProductById query,
        CatalogDbContext dbContext,
        CancellationToken ct)
    {
        var product = await dbContext.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .SingleOrDefaultAsync(p => p.Id == query.Id && p.IsActive, ct);

        if (product is null)
        {
            return Error.NotFound("Product.NotFound", $"Product with ID '{query.Id}' was not found.");
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
