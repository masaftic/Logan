using BuildingBlocks.Common.Results;
using Inventory.Api.Domain.Enums;

namespace Inventory.Api.Domain.Errors;

public static class InventoryErrors
{
    public static Error InsufficientStock(string sku, int requested, int available) =>
        Error.Conflict(
            code: "Inventory.InsufficientStock",
            description: $"Insufficient stock for SKU '{sku}'. Requested: {requested}, Available: {available}.")
        .WithMetadata("Sku", sku)
        .WithMetadata("RequestedQuantity", requested)
        .WithMetadata("AvailableQuantity", available);

    public static Error StockNotFound(string sku) =>
        Error.NotFound(
            code: "Inventory.StockNotFound",
            description: $"Stock item with SKU '{sku}' was not found.")
        .WithMetadata("Sku", sku);

    public static Error InvalidReservationState(Guid reservationId, ReservationStatus currentStatus, string targetAction) =>
        Error.Conflict(
            code: "Inventory.InvalidReservationState",
            description: $"Cannot perform '{targetAction}' on reservation '{reservationId}' because it is in state '{currentStatus}'.")
        .WithMetadata("ReservationId", reservationId)
        .WithMetadata("CurrentStatus", currentStatus.ToString())
        .WithMetadata("TargetAction", targetAction);

    public static Error ReservationNotFound(Guid orderId) =>
        Error.NotFound(
            code: "Inventory.ReservationNotFound",
            description: $"No active stock reservation found for order '{orderId}'.")
        .WithMetadata("OrderId", orderId);
}
