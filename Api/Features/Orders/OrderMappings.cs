using Api.Domain.Orders;
using Api.Domain.Payments;
using Contracts.Orders.DTOs;
using Contracts.Payments.DTOs;

namespace Api.Features.Orders;

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order) =>
        new(
            order.Id,
            order.Amount,
            order.Status.ToString(),
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            order.Payments?.Select(ToResponse).ToList() ?? []
        );

    public static PaymentResponse ToResponse(this Payment payment) =>
        new(
            payment.Id,
            payment.Amount,
            payment.Status.ToString(),
            payment.GatewayTransactionId,
            payment.FailureReason,
            payment.CreatedAtUtc,
            payment.CompletedAtUtc
        );
}
