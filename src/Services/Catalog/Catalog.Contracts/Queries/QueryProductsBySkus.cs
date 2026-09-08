namespace Catalog.Contracts.Queries;

public record QueryProductsBySkus(IReadOnlyList<string> Skus);
