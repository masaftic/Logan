namespace Shipping.Contracts.DTOs;

public record TrackingMilestoneDto(
    string Status,
    string Message,
    string? Location,
    DateTime OccurredAtUtc);
