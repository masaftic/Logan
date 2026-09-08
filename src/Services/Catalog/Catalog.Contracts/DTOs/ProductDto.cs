namespace Catalog.Contracts.DTOs;

public record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    string Currency,
    decimal WeightValue,
    string WeightUnit,
    decimal Length,
    decimal Width,
    decimal Height,
    string DimensionUnit,
    Guid CategoryId,
    string CategoryName
);
