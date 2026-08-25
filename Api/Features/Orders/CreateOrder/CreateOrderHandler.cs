using Api.Data;
using Api.Domain.Orders;
using Contracts.Orders.Commands;
using Contracts.Orders.DTOs;
using Contracts.Orders.Events;

namespace Api.Features.Orders.CreateOrder;

public static class CreateOrderHandler
{
    public static (OrderResponse, OrderSubmitted) Handle(CreateOrderCommand command, OrderDbContext dbContext)
    {
        var order = Order.Create(command.Amount);

        dbContext.Orders.Add(order);

        var response = order.ToResponse();
        var @event = new OrderSubmitted(order.Id, order.Amount, order.CreatedAtUtc);

        return (response, @event);
    }
}
