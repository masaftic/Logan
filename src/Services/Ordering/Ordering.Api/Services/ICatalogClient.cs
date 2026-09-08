using BuildingBlocks.Common.Results;
using Catalog.Contracts.DTOs;

namespace Ordering.Api.Services;

public interface ICatalogClient
{
    Task<Result<IReadOnlyList<ProductDto>>> GetProductsBySkusAsync(IReadOnlyList<string> skus, CancellationToken cancellationToken = default);
}
