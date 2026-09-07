using Shipping.Api.Domain;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Enums;

namespace Shipping.Api.Features;

public static class ShipmentDtoExtensions
{
    public static ShipmentDetailsDto ToDetailsDto(this Shipment shipment) => new(
        Id: shipment.Id,
        OrderId: shipment.OrderId,
        Status: (ShippingStatusDto)shipment.Status,
        OriginAddress: new ShippingAddressDto(
            Street1: shipment.OriginAddress.Street1,
            Street2: shipment.OriginAddress.Street2,
            City: shipment.OriginAddress.City,
            State: shipment.OriginAddress.State,
            PostalCode: shipment.OriginAddress.PostalCode,
            Country: shipment.OriginAddress.Country.Key),
        DestinationAddress: new ShippingAddressDto(
            Street1: shipment.DestinationAddress.Street1,
            Street2: shipment.DestinationAddress.Street2,
            City: shipment.DestinationAddress.City,
            State: shipment.DestinationAddress.State,
            PostalCode: shipment.DestinationAddress.PostalCode,
            Country: shipment.DestinationAddress.Country.Key),
        Dimensions: new PackageDimensionsDto(
            Length: shipment.Dimensions.Length.Value,
            Width: shipment.Dimensions.Width.Value,
            Height: shipment.Dimensions.Height.Value,
            Unit: shipment.Dimensions.Length.Unit.Key),
        Weight: new PackageWeightDto(
            Value: shipment.Weight.Value,
            Unit: shipment.Weight.Unit.Key),
        Carrier: shipment.SelectedRate?.Carrier,
        Service: shipment.SelectedRate?.Service,
        RateAmount: shipment.SelectedRate?.Price,
        Currency: shipment.SelectedRate?.Currency,
        TrackingNumber: shipment.TrackingNumber,
        LabelUrl: shipment.LabelUrl,
        ProviderShipmentId: shipment.ProviderShipmentId,
        ProviderTrackerId: shipment.ProviderTrackerId,
        CreatedAtUtc: shipment.CreatedAtUtc,
        DispatchedAtUtc: shipment.DispatchedAtUtc,
        DeliveredAtUtc: shipment.DeliveredAtUtc,
        CancelledAtUtc: shipment.CancelledAtUtc,
        FailureReason: shipment.FailureReason,
        Items: shipment.Items.Select(i => new ShipmentItemDto(i.Sku, i.Quantity)).ToList(),
        TrackingMilestones: shipment.TrackingMilestones.Select(m => new TrackingMilestoneDto(
            Status: m.Status,
            Message: m.Message,
            Location: m.Location,
            OccurredAtUtc: m.OccurredAtUtc)).ToList());
}
