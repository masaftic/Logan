namespace Inventory.Contracts.Queries;

public record GetStockListQuery(
    IReadOnlyList<string>? Skus = null
);

