using Api.Data;
using Api.Domain.Orders;
using Contracts.Orders.Events;
using Contracts.Payments.Events;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Api.Features.Orders;

public class OrderSaga : Saga
{
    // Primary Key / Correlation ID (Matches OrderId)
    public Guid Id { get; set; }

    public OrderStatus Status { get; set; }
    public decimal Amount { get; set; }
    public Guid? PaymentId { get; set; }
    public string? FailureReason { get; set; }

    // Timestamps
    public DateTime StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? FailedAtUtc { get; set; }

    public static (OrderSaga, OrderTimeout) Start(OrderSubmitted @event, ILogger<OrderSaga> logger)
    {
        logger.LogInformation("OrderSaga for Order {OrderId} started.", @event.OrderId);

        var initialState = new OrderSaga
        {
            Id = @event.OrderId,
            Status = OrderStatus.Processing,
            Amount = @event.Amount,
            StartedAtUtc = DateTime.UtcNow
        };

        return (
            initialState, 
            new OrderTimeout(@event.OrderId)
        );
    }

    public async Task Handle(
        PaymentCompleted @event,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found when applying PaymentCompleted in Saga.", @event.OrderId);
            return;
        }

        order.Complete(@event.PaymentId);
        await dbContext.SaveChangesAsync(ct);

        Status = OrderStatus.Completed;
        PaymentId = @event.PaymentId;
        CompletedAtUtc = DateTime.UtcNow;

        logger.LogInformation("Order {OrderId} successfully completed by Saga (PaymentId: {PaymentId}).",
            order.Id, @event.PaymentId);

        MarkCompleted();
    }

    public async Task Handle(
        PaymentFailed @event,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found when applying PaymentFailed in Saga.", @event.OrderId);
            return;
        }

        order.Fail(@event.Reason);
        await dbContext.SaveChangesAsync(ct);

        Status = OrderStatus.Failed;
        FailureReason = @event.Reason;
        FailedAtUtc = DateTime.UtcNow;

        logger.LogWarning("Order {OrderId} marked as Failed by Saga. Reason: {Reason}",
            order.Id, @event.Reason);

        MarkCompleted();
    }

    public async Task Handle(
        OrderTimeout @event,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId, ct);
        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found when applying OrderTimeout in Saga.", @event.OrderId);
            return;
        }

        if (order.Status == OrderStatus.Completed)
        {
            logger.LogInformation("Order {OrderId} was already completed when OrderTimeout fired. Ignoring.", @event.OrderId);
            return;
        }

        order.Timeout();
        await dbContext.SaveChangesAsync(ct);

        Status = OrderStatus.TimedOut;
        FailureReason = "Payment window expired (timeout).";
        FailedAtUtc = DateTime.UtcNow;

        logger.LogWarning("Order {OrderId} timed out in Saga awaiting payment.", order.Id);

        MarkCompleted();
    }
}
