using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Ordering.Api.Data;
using Ordering.Api.Domain;
using Ordering.Contracts.Commands;
using Ordering.Contracts.DTOs;
using Ordering.Contracts.Enums;
using Ordering.Contracts.Events;
using Wolverine;

namespace Ordering.Api.Features.SubmitOrder;

public static class SubmitOrderHandler
{
    public static async Task<Result<OrderDto>> Handle(
        SubmitOrderCommand command,
        OrderDbContext dbContext,
        IMessageBus bus,
        CancellationToken ct)
    {
        var orderId = Guid.CreateVersion7();

        List<OrderItem> items = [.. command.Items.Select(
            i => OrderItem.Create(
                    Sku.Create(i.Sku),
                    PositiveQuantity.Create(i.Quantity),
                    Price.Create(i.UnitPrice)))];

        var order = Order.Create(orderId, command.CustomerId, items);
        dbContext.Orders.Add(order);

        List<OrderItemDto> itemDtos = [.. order.Items.Select(i => new OrderItemDto(
            i.Sku,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice
        ))];

        await bus.PublishAsync(new OrderSubmittedEvent(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            itemDtos,
            order.CreatedAtUtc
        ));

        var dto = new OrderDto(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            (OrderStatusDto)order.Status,
            itemDtos,
            order.CreatedAtUtc,
            order.CompletedAtUtc,
            order.CancelledAtUtc,
            order.CancellationReason
        );

        return dto;
    }
}
