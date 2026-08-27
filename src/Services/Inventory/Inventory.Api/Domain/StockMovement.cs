using Inventory.Api.Domain.Enums;
using Inventory.Api.Domain.ValueObjects;

namespace Inventory.Api.Domain;

public class StockMovement
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; } = null!;
    public int QuantityDelta { get; private set; }
    public Quantity AvailableAfter { get; private set; }
    public Quantity ReservedAfter { get; private set; }
    public StockMovementType Type { get; private set; }
    public string? ReferenceId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(
        string sku,
        int quantityDelta,
        Quantity availableAfter,
        Quantity reservedAfter,
        StockMovementType type,
        string? referenceId = null)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            Sku = sku,
            QuantityDelta = quantityDelta,
            AvailableAfter = availableAfter,
            ReservedAfter = reservedAfter,
            Type = type,
            ReferenceId = referenceId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
