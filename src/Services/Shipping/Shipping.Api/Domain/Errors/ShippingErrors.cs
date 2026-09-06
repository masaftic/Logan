using BuildingBlocks.Common.Results;
using Shipping.Api.Domain.Enums;

namespace Shipping.Api.Domain.Errors;

public static class ShippingErrors
{
    public static Error ShipmentNotFound(ShipmentId id) =>
        Error.NotFound(
            code: "Shipping.NotFound",
            description: $"Shipment '{id}' was not found.")
        .WithMetadata("ShipmentId", id.ToString());

    public static Error ShipmentNotFoundForOrder(Guid orderId) =>
        Error.NotFound(
            code: "Shipping.NotFoundForOrder",
            description: $"Shipment for order '{orderId}' was not found.")
        .WithMetadata("OrderId", orderId);

    public static Error CannotPurchaseLabelInCurrentStatus(ShipmentId id, ShippingStatus status) =>
        Error.Conflict(
            code: "Shipping.CannotPurchaseLabel",
            description: $"Cannot purchase shipping label for shipment '{id}' in status '{status}'. Only draft shipments can purchase postage.")
        .WithMetadata("ShipmentId", id.ToString())
        .WithMetadata("CurrentStatus", status.ToString());

    public static Error CannotCancelDispatchedShipment(ShipmentId id, string currentStatus) =>
        Error.Conflict(
            code: "Shipping.CannotCancelDispatched",
            description: $"Cannot cancel shipment '{id}' because it has already been dispatched to carrier (current status: '{currentStatus}').")
        .WithMetadata("ShipmentId", id.ToString())
        .WithMetadata("CurrentStatus", currentStatus);

    public static Error EmptyShipmentItems() =>
        Error.Validation(
            code: "Shipping.EmptyItems",
            description: "A shipment must contain at least one item.");

    public static Error ShippoError(string message) =>
        Error.ExternalService(
            code: "Shipping.ShippoError",
            description: $"Shippo shipping provider error: {message}");

    public static Error AddressVerificationFailed(string details) =>
        Error.Validation(
            code: "Shipping.AddressVerificationFailed",
            description: $"Destination address verification failed: {details}");
}
