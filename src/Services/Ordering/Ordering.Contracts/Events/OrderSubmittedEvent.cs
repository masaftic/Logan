using Ordering.Contracts.DTOs;
using Shipping.Contracts.DTOs;

namespace Ordering.Contracts.Events;

public record OrderSubmittedEvent(
    Guid OrderId,
    Guid CustomerId,
    decimal TotalAmount,
    string Currency,
    IReadOnlyList<OrderItemDto> Items,
    DateTime CreatedAtUtc,
    string ProviderRateId,
    ShippingAddressDto DestinationAddress
);

