using Ordering.Contracts.DTOs;
using Payment.Contracts.DTOs;

namespace ApiGateway.Features.Checkout;

public record CheckoutRequest(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequestDto> Items,
    string Currency = "usd"
);

public record CheckoutResponse(
    OrderDto Order,
    PaymentInitializationDto Payment
);
