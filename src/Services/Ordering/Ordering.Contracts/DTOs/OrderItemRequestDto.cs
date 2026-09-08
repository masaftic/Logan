namespace Ordering.Contracts.DTOs;

public record OrderItemRequestDto(
    string Sku,
    int Quantity
);
