using Inventory.Contracts.Commands;
using Microsoft.EntityFrameworkCore;
using Ordering.Api.Data;
using Ordering.Api.Domain.Enums;
using Ordering.Contracts.DTOs;
using Ordering.Contracts.Events;
using Payment.Contracts.Commands;
using Payment.Contracts.Events;
using Shipping.Contracts.Commands;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Events;
using Wolverine;

namespace Ordering.Api.Features.OrderSaga;

public class OrderSaga : Saga
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = null!;
    public OrderSagaStatus Status { get; set; }
    public string? FailureReason { get; set; }

    public string ProviderRateId { get; set; } = null!;
    public ShippingAddressDto DestinationAddress { get; set; } = null!;
    public List<ShipmentItemDto> ShipmentItems { get; set; } = [];

    public string? PaymentIntentId { get; set; }
    public Guid? ShipmentId { get; set; }
    public string? TrackingNumber { get; set; }

    public DateTime StartedAtUtc { get; set; }
    public DateTime? PaymentCompletedAtUtc { get; set; }
    public DateTime? DispatchedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }

    public static async Task<(OrderSaga, InitializePaymentCommand)> Start(
        OrderSubmittedEvent @event,
        IMessageBus bus,
        ILogger<OrderSaga> logger)
    {
        logger.LogInformation("Starting OrderSaga for Order {OrderId}.", @event.OrderId);

        List<ShipmentItemDto> shipmentItems = [.. @event.Items.Select(
            i => new ShipmentItemDto(i.Sku, i.Quantity))];

        var saga = new OrderSaga
        {
            Id = @event.OrderId,
            CustomerId = @event.CustomerId,
            TotalAmount = @event.TotalAmount,
            Currency = @event.Currency,
            Status = OrderSagaStatus.AwaitingPayment,
            ProviderRateId = @event.ProviderRateId,
            DestinationAddress = @event.DestinationAddress,
            ShipmentItems = shipmentItems,
            StartedAtUtc = @event.CreatedAtUtc
        };

        await bus.ScheduleAsync(
            new OrderPaymentTimeout(saga.Id),
            TimeSpan.FromMinutes(15));

        var initializePayment = new InitializePaymentCommand(
            OrderId: saga.Id,
            CustomerId: saga.CustomerId,
            Amount: saga.TotalAmount,
            Currency: saga.Currency);

        return (saga, initializePayment);
    }

    public async Task<(ConfirmStockDeductionCommand, CommandCreateShipment, OrderConfirmedEvent)> Handle(
        PaymentCompletedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        PaymentIntentId = @event.PaymentIntentId;
        PaymentCompletedAtUtc = @event.CompletedAtUtc;
        Status = OrderSagaStatus.AwaitingShipment;

        var orderSummary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == Id, ct);
        if (orderSummary is not null)
        {
            orderSummary.UpdatePaymentStatus(PaymentStatus.Captured);
        }

        var order = await dbContext.Orders
            .Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == Id, ct);

        await dbContext.SaveChangesAsync(ct);

        var confirmStock = new ConfirmStockDeductionCommand(Id);

        var createShipment = new CommandCreateShipment(
            OrderId: Id,
            ProviderRateId: ProviderRateId,
            DestinationAddress: DestinationAddress,
            Items: ShipmentItems);

        List<OrderItemDto> itemDtos = order is not null
            ? [.. order.Items.Select(i => new OrderItemDto(i.Sku, i.Quantity, i.UnitPrice, i.TotalPrice))]
            : [];

        var orderConfirmed = new OrderConfirmedEvent(
            OrderId: Id,
            CustomerId: CustomerId,
            TotalAmount: TotalAmount,
            Currency: Currency,
            Items: itemDtos,
            DestinationAddress: DestinationAddress,
            PaymentIntentId: @event.PaymentIntentId,
            ConfirmedAtUtc: @event.CompletedAtUtc);

        logger.LogInformation("Payment completed for Order {OrderId}. Publishing OrderConfirmedEvent and dispatching stock confirmation and shipment label purchase.", Id);

        return (confirmStock, createShipment, orderConfirmed);
    }

    public async Task<OrderCompletedEvent> Handle(
        ShipmentLabelPurchasedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        ShipmentId = @event.ShipmentId;
        TrackingNumber = @event.TrackingNumber;
        DispatchedAtUtc = @event.DispatchedAtUtc;
        CompletedAtUtc = DateTime.UtcNow;
        Status = OrderSagaStatus.Completed;

        var order = await dbContext.Orders.SingleOrDefaultAsync(o => o.Id == Id, ct);
        order?.Complete();

        var orderSummary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == Id, ct);
        if (orderSummary is not null)
        {
            orderSummary.UpdateShippingStatus(ShippingStatus.Dispatched);
            orderSummary.MarkCompleted(CompletedAtUtc.Value);
        }

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} fulfilled and completed successfully. Carrier: {Carrier}, Tracking: {TrackingNumber}",
            Id, @event.Carrier, @event.TrackingNumber);

        MarkCompleted();

        return new OrderCompletedEvent(Id, CompletedAtUtc.Value);
    }

    public async Task<ReleaseStockCommand> Handle(
        PaymentFailedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        FailureReason = $"Payment failed: {@event.DeclineReason} ({@event.ErrorCode})";
        CancelledAtUtc = DateTime.UtcNow;
        Status = OrderSagaStatus.Failed;

        var order = await dbContext.Orders.SingleOrDefaultAsync(o => o.Id == Id, ct);
        order?.Cancel(FailureReason);

        var orderSummary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == Id, ct);
        if (orderSummary is not null)
        {
            orderSummary.UpdatePaymentStatus(PaymentStatus.Failed);
            orderSummary.MarkCancelled(CancelledAtUtc.Value);
        }

        await dbContext.SaveChangesAsync(ct);

        logger.LogWarning("Payment failed for Order {OrderId}. Triggering stock release compensation. Reason: {Reason}",
            Id, FailureReason);

        MarkCompleted();

        return new ReleaseStockCommand(Id, FailureReason);
    }

    public async Task<(RefundPaymentCommand, ReleaseStockCommand)> Handle(
        ShipmentCreationFailedEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        FailureReason = $"Shipping creation failed: {@event.Reason}";
        CancelledAtUtc = DateTime.UtcNow;
        Status = OrderSagaStatus.Failed;

        var order = await dbContext.Orders.SingleOrDefaultAsync(o => o.Id == Id, ct);
        order?.Cancel(FailureReason);

        var orderSummary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == Id, ct);
        if (orderSummary is not null)
        {
            orderSummary.UpdateShippingStatus(ShippingStatus.Failed);
            orderSummary.UpdatePaymentStatus(PaymentStatus.Refunded);
            orderSummary.MarkCancelled(CancelledAtUtc.Value);
        }

        await dbContext.SaveChangesAsync(ct);

        logger.LogWarning("Shipping failed for Order {OrderId} after payment. Triggering dual compensations (Refund + Stock Release). Reason: {Reason}",
            Id, FailureReason);

        MarkCompleted();

        var refundCommand = new RefundPaymentCommand(Id, TotalAmount, FailureReason);
        var releaseStockCommand = new ReleaseStockCommand(Id, FailureReason);

        return (refundCommand, releaseStockCommand);
    }

    public async Task<ReleaseStockCommand?> Handle(
        OrderPaymentTimeout timeout,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        if (PaymentCompletedAtUtc.HasValue || Status == OrderSagaStatus.Completed || Status == OrderSagaStatus.Failed)
        {
            logger.LogInformation("OrderPaymentTimeout fired for Order {OrderId}, but order is already in state '{Status}'. No action required.",
                Id, Status);
            return null;
        }

        FailureReason = "Payment window expired (15-minute hold TTL).";
        CancelledAtUtc = DateTime.UtcNow;
        Status = OrderSagaStatus.TimedOut;

        var order = await dbContext.Orders.SingleOrDefaultAsync(o => o.Id == Id, ct);
        order?.Cancel(FailureReason);

        var orderSummary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == Id, ct);
        if (orderSummary is not null)
        {
            orderSummary.UpdatePaymentStatus(PaymentStatus.Failed);
            orderSummary.MarkCancelled(CancelledAtUtc.Value);
        }

        await dbContext.SaveChangesAsync(ct);

        logger.LogWarning("Order {OrderId} timed out waiting for payment. Releasing reserved stock hold.", Id);

        MarkCompleted();

        return new ReleaseStockCommand(Id, FailureReason);
    }

    public async Task<object[]> Handle(
        OrderCancelledEvent @event,
        OrderDbContext dbContext,
        ILogger<OrderSaga> logger,
        CancellationToken ct)
    {
        if (Status == OrderSagaStatus.Completed || Status == OrderSagaStatus.Failed || Status == OrderSagaStatus.TimedOut)
        {
            return [];
        }

        FailureReason = $"Order manually cancelled: {@event.Reason}";
        CancelledAtUtc = @event.CancelledAtUtc;
        Status = OrderSagaStatus.Failed;

        var order = await dbContext.Orders.SingleOrDefaultAsync(o => o.Id == Id, ct);
        order?.Cancel(FailureReason);

        var orderSummary = await dbContext.OrderSummaries.SingleOrDefaultAsync(s => s.OrderId == Id, ct);
        if (orderSummary is not null)
        {
            if (PaymentCompletedAtUtc.HasValue)
            {
                orderSummary.UpdatePaymentStatus(PaymentStatus.Refunded);
            }
            orderSummary.MarkCancelled(CancelledAtUtc.Value);
        }

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("Order {OrderId} cancelled while saga was active. Applying compensations.", Id);

        MarkCompleted();

        List<object> compensations = [];
        if (PaymentCompletedAtUtc.HasValue)
        {
            compensations.Add(new RefundPaymentCommand(Id, TotalAmount, FailureReason));
        }

        compensations.Add(new ReleaseStockCommand(Id, FailureReason));

        return [.. compensations];
    }
}
