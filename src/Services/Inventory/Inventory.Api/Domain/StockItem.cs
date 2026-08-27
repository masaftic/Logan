using BuildingBlocks.Common.Results;
using Inventory.Api.Domain.Enums;
using Inventory.Api.Domain.Errors;
using Inventory.Api.Domain.ValueObjects;

namespace Inventory.Api.Domain;

public class StockItem
{
    public string Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public Quantity QuantityAvailable { get; private set; }
    public Quantity QuantityReserved { get; private set; }
    public Quantity QuantityOnHand { get; private set; }
    public uint Version { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private StockItem() { }

    public static Result<StockItem> Create(string sku, string name, Quantity initialQuantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            return Error.Validation("Sku", "SKU cannot be empty.");
        }

        return new StockItem
        {
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name,
            QuantityAvailable = initialQuantity,
            QuantityReserved = Quantity.Zero,
            QuantityOnHand = initialQuantity,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public StockMovement Restock(PositiveQuantity quantity, string? referenceId = null)
    {
        QuantityAvailable += quantity;
        QuantityOnHand += quantity;
        LastModifiedAtUtc = DateTime.UtcNow;

        return StockMovement.Create(
            Sku,
            quantityDelta: quantity,
            availableAfter: QuantityAvailable,
            reservedAfter: QuantityReserved,
            type: StockMovementType.Restock,
            referenceId: referenceId
        );
    }

    public bool CanReserve(PositiveQuantity quantity) => QuantityAvailable >= quantity;

    public Result<StockMovement> Reserve(PositiveQuantity quantity, string? referenceId = null)
    {
        if (!CanReserve(quantity))
        {
            return InventoryErrors.InsufficientStock(Sku, (int)quantity, (int)QuantityAvailable);
        }

        QuantityAvailable -= quantity;
        QuantityReserved += quantity;
        LastModifiedAtUtc = DateTime.UtcNow;

        return StockMovement.Create(
            Sku,
            quantityDelta: -(int)quantity,
            availableAfter: QuantityAvailable,
            reservedAfter: QuantityReserved,
            type: StockMovementType.Reservation,
            referenceId: referenceId
        );
    }

    public StockMovement Release(PositiveQuantity quantity, string? referenceId = null)
    {
        QuantityReserved -= quantity;
        QuantityAvailable += quantity;
        LastModifiedAtUtc = DateTime.UtcNow;

        return StockMovement.Create(
            Sku,
            quantityDelta: (int)quantity,
            availableAfter: QuantityAvailable,
            reservedAfter: QuantityReserved,
            type: StockMovementType.Release,
            referenceId: referenceId
        );
    }

    public StockMovement ConfirmDeduction(PositiveQuantity quantity, string? referenceId = null)
    {
        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
        LastModifiedAtUtc = DateTime.UtcNow;

        return StockMovement.Create(
            Sku,
            quantityDelta: -(int)quantity,
            availableAfter: QuantityAvailable,
            reservedAfter: QuantityReserved,
            type: StockMovementType.FulfillmentDeduction,
            referenceId: referenceId
        );
    }
}
