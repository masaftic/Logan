using System.ComponentModel.DataAnnotations;
using Api.Models;

namespace Api.DTOs;

public record CreateOrderRequest(
    [Range(0.01, 100000)]
    decimal Amount);

public record OrderResponse(
    Guid Id,
    decimal Amount,
    OrderStatus Status,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc
)
{
    public static OrderResponse FromEntity(Order order) =>
        new(order.Id, order.Amount, order.Status, order.CreatedAtUtc, order.UpdatedAtUtc);
}
