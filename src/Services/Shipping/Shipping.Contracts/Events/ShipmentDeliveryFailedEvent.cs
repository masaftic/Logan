namespace Shipping.Contracts.Events;

public record ShipmentDeliveryFailedEvent(
    Guid ShipmentId,
    Guid OrderId,
    string TrackingNumber,
    string Reason,
    DateTime OccurredAtUtc);
