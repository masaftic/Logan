using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Catalog.Contracts.DTOs;
using Catalog.Contracts.Queries;

namespace Ordering.Api.Services;

public class CatalogClient(HttpClient httpClient) : ICatalogClient
{
    public Task<Result<IReadOnlyList<ProductDto>>> GetProductsBySkusAsync(IReadOnlyList<string> skus, CancellationToken cancellationToken = default) =>
        httpClient.PostAsJsonResultAsync<QueryProductsBySkus, IReadOnlyList<ProductDto>>("/api/products/by-skus", new QueryProductsBySkus(skus), cancellationToken);
}
