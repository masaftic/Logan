namespace Inventory.Contracts.DTOs;

public record StockMovementDto(
    Guid Id,
    string Sku,
    int QuantityDelta,
    int AvailableAfter,
    int ReservedAfter,
    string Type,
    string? ReferenceId,
    DateTime CreatedAtUtc
);

