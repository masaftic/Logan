namespace Ordering.Contracts.Events;

public record OrderCancelledEvent(
    Guid OrderId,
    string Reason,
    DateTime CancelledAtUtc
);
