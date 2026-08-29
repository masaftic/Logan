namespace Ordering.Contracts.DTOs;

public record OrderItemDto(
    string Sku,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
