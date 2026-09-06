using Shipping.Contracts.Enums;

namespace Shipping.Contracts.DTOs;

public record ShipmentDto(
    Guid Id,
    Guid OrderId,
    ShippingStatusDto Status,
    string? Carrier,
    string? Service,
    decimal? RateAmount,
    string? Currency,
    string? TrackingNumber,
    string? LabelUrl,
    DateTime CreatedAtUtc,
    DateTime? DispatchedAtUtc,
    string? FailureReason);
