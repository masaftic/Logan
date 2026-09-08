using Ordering.Contracts.DTOs;
using Payment.Contracts.DTOs;
using Shipping.Contracts.DTOs;

namespace ApiGateway.Features.Checkout;

public record CheckoutRequest(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequestDto> Items,
    string Currency,
    string ProviderRateId,
    ShippingAddressDto DestinationAddress,
    PackageDimensionsDto Dimensions,
    PackageWeightDto Weight
);

public record CheckoutResponse(
    OrderDto Order,
    PaymentInitializationDto Payment
);
