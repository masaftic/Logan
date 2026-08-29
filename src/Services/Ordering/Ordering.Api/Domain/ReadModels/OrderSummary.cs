using BuildingBlocks.Common.ValueObjects;
using Ordering.Api.Domain.Enums;

namespace Ordering.Api.Domain.ReadModels;

public class OrderSummary
{
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Price TotalAmount { get; private set; }
    public int TotalItemsCount { get; private set; }
    public OrderStatus OrderStatus { get; private set; }
    public InventoryStatus InventoryStatus { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public ShippingStatus ShippingStatus { get; private set; }
    public CustomerOrderStatus CustomerStatus { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private OrderSummary() { }

    public static OrderSummary Create(
        Guid orderId,
        Guid customerId,
        Price totalAmount,
        int totalItemsCount,
        DateTime createdAtUtc)
    {
        var summary = new OrderSummary
        {
            OrderId = orderId,
            CustomerId = customerId,
            TotalAmount = totalAmount,
            TotalItemsCount = totalItemsCount,
            OrderStatus = OrderStatus.Submitted,
            InventoryStatus = InventoryStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            ShippingStatus = ShippingStatus.Pending,
            CreatedAtUtc = createdAtUtc
        };

        summary.CustomerStatus = summary.ComputeCustomerStatus();
        return summary;
    }

    public void UpdateInventoryStatus(InventoryStatus status)
    {
        InventoryStatus = status;
        LastModifiedAtUtc = DateTime.UtcNow;
        CustomerStatus = ComputeCustomerStatus();
    }

    public void UpdatePaymentStatus(PaymentStatus status)
    {
        PaymentStatus = status;
        LastModifiedAtUtc = DateTime.UtcNow;
        CustomerStatus = ComputeCustomerStatus();
    }

    public void UpdateShippingStatus(ShippingStatus status)
    {
        ShippingStatus = status;
        LastModifiedAtUtc = DateTime.UtcNow;
        CustomerStatus = ComputeCustomerStatus();
    }

    public void MarkCompleted(DateTime completedAtUtc)
    {
        OrderStatus = OrderStatus.Completed;
        LastModifiedAtUtc = completedAtUtc;
        CustomerStatus = ComputeCustomerStatus();
    }

    public void MarkCancelled(DateTime cancelledAtUtc)
    {
        OrderStatus = OrderStatus.Cancelled;
        LastModifiedAtUtc = cancelledAtUtc;
        CustomerStatus = ComputeCustomerStatus();
    }

    private CustomerOrderStatus ComputeCustomerStatus()
    {
        if (OrderStatus == OrderStatus.Cancelled)
        {
            return PaymentStatus == PaymentStatus.Refunded
                ? CustomerOrderStatus.Refunded
                : CustomerOrderStatus.Cancelled;
        }

        if (OrderStatus == OrderStatus.Completed || ShippingStatus == ShippingStatus.Dispatched)
        {
            return ShippingStatus == ShippingStatus.Dispatched
                ? CustomerOrderStatus.Shipped
                : CustomerOrderStatus.Completed;
        }

        if (PaymentStatus == PaymentStatus.Captured)
        {
            return CustomerOrderStatus.PreparingForDispatch;
        }

        if (InventoryStatus == InventoryStatus.Reserved)
        {
            return CustomerOrderStatus.PaymentPending;
        }

        return CustomerOrderStatus.Processing;
    }
}
