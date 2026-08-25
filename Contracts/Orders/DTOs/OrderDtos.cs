using Contracts.Payments.DTOs;

namespace Contracts.Orders.DTOs;

public record CreateOrderRequest(decimal Amount);

public record OrderResponse(
    Guid Id,
    decimal Amount,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    List<PaymentResponse> Payments
);
