namespace Shipping.Contracts.Commands;

public record CommandProcessShippoWebhook(
    string TrackingNumber,
    string Status,
    string? Message,
    string? Location,
    DateTime? OccurredAtUtc);
