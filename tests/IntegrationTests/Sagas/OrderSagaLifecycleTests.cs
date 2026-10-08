using BuildingBlocks.Common.ValueObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Ordering.Api.Data;
using Ordering.Api.Domain;
using Ordering.Api.Domain.Enums;
using Ordering.Api.Domain.ReadModels;
using Ordering.Api.Features.OrderSaga;
using Ordering.Contracts.DTOs;
using Ordering.Contracts.Events;
using Payment.Contracts.Events;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Events;
using Wolverine;
using Xunit;

using Thinktecture;

namespace IntegrationTests.Sagas;

public class OrderSagaLifecycleTests
{
    private readonly NullLogger<OrderSaga> _logger = NullLogger<OrderSaga>.Instance;
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    private OrderDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .UseThinktectureValueConverters()
            .Options;

        return new OrderDbContext(options);
    }

    [Fact]
    public async Task Start_Should_Initialize_Saga_And_Dispatch_PaymentInitialization()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var destination = new ShippingAddressDto("123 Market St", null, "San Francisco", "CA", "94105", "US");
        var items = new List<OrderItemDto> { new("SKU-1", 2, 25.0m, 50.0m) };

        var submittedEvent = new OrderSubmittedEvent(
            orderId, customerId, 50.0m, "USD", items, DateTime.UtcNow, "rate_123", destination);

        // Act
        var (saga, initCommand) = await OrderSaga.Start(submittedEvent, _bus, _logger);

        // Assert
        saga.Should().NotBeNull();
        saga.Id.Should().Be(orderId);
        saga.CustomerId.Should().Be(customerId);
        saga.TotalAmount.Should().Be(50.0m);
        saga.Currency.Should().Be("USD");
        saga.Status.Should().Be(OrderSagaStatus.AwaitingPayment);

        initCommand.Should().NotBeNull();
        initCommand.OrderId.Should().Be(orderId);
        initCommand.Amount.Should().Be(50.0m);
        initCommand.Currency.Should().Be("USD");

        await _bus.Received(1).PublishAsync(
            Arg.Is<OrderPaymentTimeout>(t => t.OrderId == orderId),
            Arg.Any<DeliveryOptions>());
    }

    [Fact]
    public async Task PaymentCompleted_Should_Transition_To_AwaitingShipment_And_Dispatch_Shipment_And_StockConfirmation()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var destination = new ShippingAddressDto("123 Market St", null, "San Francisco", "CA", "94105", "US");

        var orderSummary = OrderSummary.Create(orderId, customerId, Price.Create(100m), 1, DateTime.UtcNow);
        dbContext.OrderSummaries.Add(orderSummary);
        await dbContext.SaveChangesAsync();

        var saga = new OrderSaga
        {
            Id = orderId,
            CustomerId = customerId,
            TotalAmount = 100m,
            Currency = "USD",
            Status = OrderSagaStatus.AwaitingPayment,
            ProviderRateId = "rate_ground",
            DestinationAddress = destination,
            ShipmentItems = [new ShipmentItemDto("SKU-1", 1)]
        };

        var paymentCompletedEvent = new PaymentCompletedEvent(
            orderId, "pi_test_123", 100m, "USD", DateTime.UtcNow);

        // Act
        var (confirmStock, createShipment, orderConfirmed) = await saga.Handle(
            paymentCompletedEvent, dbContext, _logger, CancellationToken.None);

        // Assert
        saga.Status.Should().Be(OrderSagaStatus.AwaitingShipment);
        saga.PaymentIntentId.Should().Be("pi_test_123");

        confirmStock.OrderId.Should().Be(orderId);
        createShipment.OrderId.Should().Be(orderId);
        createShipment.ProviderRateId.Should().Be("rate_ground");
        orderConfirmed.OrderId.Should().Be(orderId);

        var updatedSummary = await dbContext.OrderSummaries.SingleAsync(s => s.OrderId == orderId);
        updatedSummary.PaymentStatus.Should().Be(PaymentStatus.Captured);
    }

    [Fact]
    public async Task PaymentFailed_Should_Transition_To_Failed_And_Trigger_Stock_Release_Compensation()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderSummary = OrderSummary.Create(orderId, customerId, Price.Create(75m), 1, DateTime.UtcNow);
        dbContext.OrderSummaries.Add(orderSummary);
        await dbContext.SaveChangesAsync();

        var saga = new OrderSaga
        {
            Id = orderId,
            CustomerId = customerId,
            TotalAmount = 75m,
            Currency = "USD",
            Status = OrderSagaStatus.AwaitingPayment
        };

        var paymentFailedEvent = new PaymentFailedEvent(
            orderId, "card_declined", "Insufficient funds", DateTime.UtcNow);

        // Act
        var releaseStockCommand = await saga.Handle(
            paymentFailedEvent, dbContext, _logger, CancellationToken.None);

        // Assert
        saga.Status.Should().Be(OrderSagaStatus.Failed);
        saga.FailureReason.Should().Contain("Insufficient funds");
        releaseStockCommand.OrderId.Should().Be(orderId);
        releaseStockCommand.Reason.Should().Contain("Insufficient funds");

        var updatedSummary = await dbContext.OrderSummaries.SingleAsync(s => s.OrderId == orderId);
        updatedSummary.PaymentStatus.Should().Be(PaymentStatus.Failed);
        updatedSummary.OrderStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task ShippingFailed_After_Payment_Should_Trigger_Dual_Compensations_Refund_And_StockRelease()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderSummary = OrderSummary.Create(orderId, customerId, Price.Create(120m), 1, DateTime.UtcNow);
        dbContext.OrderSummaries.Add(orderSummary);
        await dbContext.SaveChangesAsync();

        var saga = new OrderSaga
        {
            Id = orderId,
            CustomerId = customerId,
            TotalAmount = 120m,
            Currency = "USD",
            Status = OrderSagaStatus.AwaitingShipment,
            PaymentIntentId = "pi_paid_456"
        };

        var shippingFailedEvent = new ShipmentCreationFailedEvent(
            orderId, "Carrier address validation failed", DateTime.UtcNow);

        // Act
        var (refundCommand, releaseStockCommand) = await saga.Handle(
            shippingFailedEvent, dbContext, _logger, CancellationToken.None);

        // Assert
        saga.Status.Should().Be(OrderSagaStatus.Failed);
        saga.FailureReason.Should().Contain("Carrier address validation failed");

        refundCommand.OrderId.Should().Be(orderId);
        refundCommand.Amount.Should().Be(120m);

        releaseStockCommand.OrderId.Should().Be(orderId);

        var updatedSummary = await dbContext.OrderSummaries.SingleAsync(s => s.OrderId == orderId);
        updatedSummary.ShippingStatus.Should().Be(ShippingStatus.Failed);
        updatedSummary.PaymentStatus.Should().Be(PaymentStatus.Refunded);
        updatedSummary.OrderStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task PaymentTimeout_Should_Cancel_Saga_And_Release_Stock_Hold()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();

        var orderSummary = OrderSummary.Create(orderId, customerId, Price.Create(90m), 1, DateTime.UtcNow);
        dbContext.OrderSummaries.Add(orderSummary);
        await dbContext.SaveChangesAsync();

        var saga = new OrderSaga
        {
            Id = orderId,
            CustomerId = customerId,
            TotalAmount = 90m,
            Currency = "USD",
            Status = OrderSagaStatus.AwaitingPayment
        };

        var timeout = new OrderPaymentTimeout(orderId);

        // Act
        var releaseStockCommand = await saga.Handle(
            timeout, dbContext, _logger, CancellationToken.None);

        // Assert
        releaseStockCommand.Should().NotBeNull();
        releaseStockCommand!.OrderId.Should().Be(orderId);
        saga.Status.Should().Be(OrderSagaStatus.TimedOut);

        var updatedSummary = await dbContext.OrderSummaries.SingleAsync(s => s.OrderId == orderId);
        updatedSummary.OrderStatus.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public async Task ShipmentPurchased_Should_Complete_Saga_Successfully()
    {
        // Arrange
        using var dbContext = CreateDbContext();
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var shipmentId = Guid.NewGuid();

        var orderSummary = OrderSummary.Create(orderId, customerId, Price.Create(60m), 1, DateTime.UtcNow);
        dbContext.OrderSummaries.Add(orderSummary);
        await dbContext.SaveChangesAsync();

        var saga = new OrderSaga
        {
            Id = orderId,
            CustomerId = customerId,
            TotalAmount = 60m,
            Currency = "USD",
            Status = OrderSagaStatus.AwaitingShipment,
            PaymentIntentId = "pi_paid_789"
        };

        var labelPurchasedEvent = new ShipmentLabelPurchasedEvent(
            ShipmentId: shipmentId,
            OrderId: orderId,
            TrackingNumber: "1Z9999999999999999",
            Carrier: "UPS",
            LabelUrl: "https://shippo.mock/label.pdf",
            DispatchedAtUtc: DateTime.UtcNow);

        // Act
        var completedEvent = await saga.Handle(
            labelPurchasedEvent, dbContext, _logger, CancellationToken.None);

        // Assert
        saga.Status.Should().Be(OrderSagaStatus.Completed);
        saga.TrackingNumber.Should().Be("1Z9999999999999999");
        completedEvent.OrderId.Should().Be(orderId);

        var updatedSummary = await dbContext.OrderSummaries.SingleAsync(s => s.OrderId == orderId);
        updatedSummary.ShippingStatus.Should().Be(ShippingStatus.Dispatched);
        updatedSummary.OrderStatus.Should().Be(OrderStatus.Completed);
    }
}
