namespace Shipping.Contracts.Events;

public record ShipmentLabelPurchasedEvent(
    Guid ShipmentId,
    Guid OrderId,
    string TrackingNumber,
    string Carrier,
    string LabelUrl,
    DateTime DispatchedAtUtc);
