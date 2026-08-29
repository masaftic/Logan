using Ordering.Contracts.Enums;

namespace Ordering.Contracts.DTOs;

public record OrderSummaryDto(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    int TotalItemsCount,
    OrderStatusDto OrderStatus,
    InventoryStatusDto InventoryStatus,
    PaymentStatusDto PaymentStatus,
    ShippingStatusDto ShippingStatus,
    CustomerOrderStatusDto CustomerStatus,
    DateTime CreatedAtUtc,
    DateTime? LastModifiedAtUtc
);
