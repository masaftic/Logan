using BuildingBlocks.Common.ValueObjects;

namespace Shipping.Api.Domain;

public class ShipmentItem
{
    public ShipmentItemId Id { get; private set; }
    public ShipmentId ShipmentId { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public PositiveQuantity Quantity { get; private set; }

    private ShipmentItem() { }

    public static ShipmentItem Create(
        ShipmentItemId id,
        ShipmentId shipmentId,
        Sku sku,
        PositiveQuantity quantity)
    {
        return new ShipmentItem
        {
            Id = id,
            ShipmentId = shipmentId,
            Sku = sku,
            Quantity = quantity
        };
    }
}
