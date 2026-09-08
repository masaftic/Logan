using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Inventory.Contracts.Commands;
using Inventory.Contracts.DTOs;
using Ordering.Api.Data;
using Ordering.Api.Domain;
using Ordering.Api.Domain.Enums;
using Ordering.Api.Domain.ReadModels;
using Ordering.Api.Services;
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
        IInventoryClient inventoryClient,
        ICatalogClient catalogClient,
        IMessageBus bus,
        CancellationToken ct)
    {
        var orderId = Guid.CreateVersion7();

        var uniqueSkus = command.Items.Select(i => i.Sku.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var productsResult = await catalogClient.GetProductsBySkusAsync(uniqueSkus, ct);
        if (productsResult.IsError)
        {
            return productsResult.Errors;
        }

        var productsBySku = productsResult.Value.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);

        foreach (var item in command.Items)
        {
            if (!productsBySku.TryGetValue(item.Sku.Trim(), out var product))
            {
                return Error.NotFound("Catalog.ProductNotFound", $"Product with SKU '{item.Sku}' was not found in catalog.");
            }

            if (!string.Equals(product.Currency, command.Currency, StringComparison.OrdinalIgnoreCase))
            {
                return Error.Validation("Catalog.CurrencyMismatch", $"Product '{item.Sku}' currency '{product.Currency}' does not match order currency '{command.Currency}'.");
            }
        }

        List<StockReservationItemDto> reservationItems = [.. command.Items.Select(
            i => new StockReservationItemDto(i.Sku, i.Quantity))];

        var reserveCommand = new ReserveStockCommand(orderId, reservationItems, HoldDurationMinutes: 15);
        
        var reservationResult = await inventoryClient.ReserveStockAsync(reserveCommand, ct);
        if (reservationResult.IsError)
        {
            return reservationResult.Errors;
        }

        List<OrderItem> items = [.. command.Items.Select(
            i => OrderItem.Create(
                    Sku.Create(i.Sku),
                    PositiveQuantity.Create(i.Quantity),
                    Price.Create(productsBySku[i.Sku.Trim()].Price)))];

        var order = Order.Create(orderId, command.CustomerId, CurrencyCode.Create(command.Currency), items);
        dbContext.Orders.Add(order);

        var orderSummary = OrderSummary.Create(
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.Items.Count,
            order.CreatedAtUtc);
        orderSummary.UpdateInventoryStatus(InventoryStatus.Reserved);
        dbContext.OrderSummaries.Add(orderSummary);

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
            command.Currency,
            itemDtos,
            order.CreatedAtUtc,
            command.ProviderRateId,
            command.DestinationAddress
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
