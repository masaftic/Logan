using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Ordering.Api.Domain.Enums;

namespace Ordering.Api.Domain;

public class Order
{
    private readonly List<OrderItem> _items = [];

    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Price TotalAmount { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? CancellationReason { get; private set; }

    private Order() { }

    public static Order Create(Guid id, Guid customerId, IReadOnlyList<OrderItem> items)
    {
        if (items.Count == 0)
            throw new ArgumentException("Order must have at least one item.", nameof(items));

        var order = new Order
        {
            Id = id,
            CustomerId = customerId,
            Status = OrderStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow,
            TotalAmount = (Price)items.Sum(i => i.TotalPrice)
        };

        order._items.AddRange(items);
        return order;
    }

    public Result Complete()
    {
        if (Status != OrderStatus.Submitted)
        {
            return Error.Conflict("Order.InvalidState", $"Cannot complete order in state '{Status}'.");
        }

        Status = OrderStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        return Result.Ok();
    }

    public Result Cancel(string reason)
    {
        if (Status == OrderStatus.Completed)
        {
            return Error.Conflict("Order.InvalidState", "Cannot cancel an already completed order.");
        }

        if (Status == OrderStatus.Cancelled)
        {
            return Result.Ok();
        }

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = DateTime.UtcNow;
        CancellationReason = reason;
        return Result.Ok();
    }
}
