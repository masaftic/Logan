using Ordering.Contracts.DTOs;
using Payment.Contracts.DTOs;
using Shipping.Contracts.DTOs;

namespace ApiGateway.Features.Checkout;

public record CheckoutRequest(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequestDto> Items,
    string Currency,
    string ProviderRateId,
    ShippingAddressDto DestinationAddress
);

public record CheckoutResponse(
    OrderDto Order,
    PaymentInitializationDto Payment
);
