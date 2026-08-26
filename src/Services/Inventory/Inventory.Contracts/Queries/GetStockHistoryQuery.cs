namespace Inventory.Contracts.Queries;

public record GetStockHistoryQuery(
    string Sku,
    int Limit = 50
);

