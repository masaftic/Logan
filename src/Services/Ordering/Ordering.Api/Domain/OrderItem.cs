using BuildingBlocks.Common.ValueObjects;

namespace Ordering.Api.Domain;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public PositiveQuantity Quantity { get; private set; }
    public Price UnitPrice { get; private set; }
    public Price TotalPrice => UnitPrice * Quantity;

    private OrderItem() { }

    public static OrderItem Create(Sku sku, PositiveQuantity quantity, Price unitPrice)
    {
        return new OrderItem
        {
            Id = Guid.CreateVersion7(),
            Sku = sku,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
