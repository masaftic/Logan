using Ordering.Contracts.DTOs;
using Shipping.Contracts.DTOs;

namespace Ordering.Contracts.Events;

public record OrderConfirmedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderItemDto> Items,
    ShippingAddressDto DestinationAddress,
    string PaymentIntentId,
    DateTime ConfirmedAtUtc
);
