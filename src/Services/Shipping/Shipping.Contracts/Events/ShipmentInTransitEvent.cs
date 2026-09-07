namespace Shipping.Contracts.Events;

public record ShipmentInTransitEvent(
    Guid ShipmentId,
    Guid OrderId,
    string TrackingNumber,
    string? Location,
    DateTime OccurredAtUtc);
