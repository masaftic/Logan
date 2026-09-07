namespace Shipping.Contracts.Events;

public record ShipmentDeliveredEvent(
    Guid ShipmentId,
    Guid OrderId,
    string TrackingNumber,
    DateTime DeliveredAtUtc);
