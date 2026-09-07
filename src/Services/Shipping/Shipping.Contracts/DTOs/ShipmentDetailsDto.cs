using Shipping.Contracts.Enums;

namespace Shipping.Contracts.DTOs;

public record ShipmentDetailsDto(
    Guid Id,
    Guid OrderId,
    ShippingStatusDto Status,
    ShippingAddressDto OriginAddress,
    ShippingAddressDto DestinationAddress,
    PackageDimensionsDto Dimensions,
    PackageWeightDto Weight,
    string? Carrier,
    string? Service,
    decimal? RateAmount,
    string? Currency,
    string? TrackingNumber,
    string? LabelUrl,
    string? ProviderShipmentId,
    string? ProviderTrackerId,
    DateTime CreatedAtUtc,
    DateTime? DispatchedAtUtc,
    DateTime? DeliveredAtUtc,
    DateTime? CancelledAtUtc,
    string? FailureReason,
    IReadOnlyList<ShipmentItemDto> Items,
    IReadOnlyList<TrackingMilestoneDto> TrackingMilestones);
