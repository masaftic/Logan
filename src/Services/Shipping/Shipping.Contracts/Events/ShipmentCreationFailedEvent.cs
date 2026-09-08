namespace Shipping.Contracts.Events;

public record ShipmentCreationFailedEvent(
    Guid OrderId,
    string Reason,
    DateTime FailedAtUtc);
