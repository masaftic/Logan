namespace Ordering.Contracts.Events;

public record OrderCompletedEvent(
    Guid OrderId,
    DateTime CompletedAtUtc
);
