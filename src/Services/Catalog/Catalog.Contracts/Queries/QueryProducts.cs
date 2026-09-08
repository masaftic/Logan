namespace Catalog.Contracts.Queries;

public record QueryProducts(
    Guid? CategoryId = null,
    int PageNumber = 1,
    int PageSize = 20
);
