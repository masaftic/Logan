using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Shipping.Api.Domain.Enums;
using Shipping.Api.Domain.Errors;
using Shipping.Api.Domain.ValueObjects;

namespace Shipping.Api.Domain;

public class Shipment
{
    private readonly List<ShipmentItem> _items = [];
    private readonly List<TrackingMilestone> _trackingMilestones = [];

    public ShipmentId Id { get; private set; }
    public Guid OrderId { get; private set; }
    public ShippingStatus Status { get; private set; }
    public Address OriginAddress { get; private set; } = null!;
    public Address DestinationAddress { get; private set; } = null!;
    public PackageDimensions Dimensions { get; private set; } = null!;
    public Weight Weight { get; private set; } = null!;
    public ShippingRateQuote? SelectedRate { get; private set; }
    public TrackingNumber? TrackingNumber { get; private set; }
    public string? LabelUrl { get; private set; }
    public string? ProviderShipmentId { get; private set; }
    public string? ProviderTrackerId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? DispatchedAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public string? FailureReason { get; private set; }

    public IReadOnlyCollection<ShipmentItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<TrackingMilestone> TrackingMilestones => _trackingMilestones.AsReadOnly();

    private Shipment() { }

    public static Result<Shipment> CreateDraft(
        ShipmentId id,
        Guid orderId,
        Address originAddress,
        Address destinationAddress,
        PackageDimensions dimensions,
        Weight weight,
        IEnumerable<ShipmentItem> items)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return ShippingErrors.EmptyShipmentItems();
        }

        var shipment = new Shipment
        {
            Id = id,
            OrderId = orderId,
            Status = ShippingStatus.Draft,
            OriginAddress = originAddress,
            DestinationAddress = destinationAddress,
            Dimensions = dimensions,
            Weight = weight,
            CreatedAtUtc = DateTime.UtcNow
        };

        shipment._items.AddRange(itemList);
        return shipment;
    }

    public Result PurchaseLabel(
        ShippingRateQuote selectedRate,
        TrackingNumber trackingNumber,
        string labelUrl,
        string providerShipmentId,
        string providerTrackerId)
    {
        if (Status != ShippingStatus.Draft)
        {
            return ShippingErrors.CannotPurchaseLabelInCurrentStatus(Id, Status);
        }

        SelectedRate = selectedRate;
        TrackingNumber = trackingNumber;
        LabelUrl = labelUrl;
        ProviderShipmentId = providerShipmentId;
        ProviderTrackerId = providerTrackerId;
        Status = ShippingStatus.LabelPurchased;
        DispatchedAtUtc = DateTime.UtcNow;

        return Result.Ok();
    }

    public void MarkInTransit(DateTime occurredAtUtc)
    {
        if (Status is ShippingStatus.InTransit or ShippingStatus.OutForDelivery or ShippingStatus.Delivered)
        {
            return;
        }

        Status = ShippingStatus.InTransit;
        DispatchedAtUtc ??= occurredAtUtc;
    }

    public void MarkOutForDelivery()
    {
        if (Status is ShippingStatus.OutForDelivery or ShippingStatus.Delivered)
        {
            return;
        }

        Status = ShippingStatus.OutForDelivery;
    }

    public void MarkDelivered(DateTime deliveredAtUtc)
    {
        if (Status == ShippingStatus.Delivered)
        {
            return;
        }

        Status = ShippingStatus.Delivered;
        DeliveredAtUtc = deliveredAtUtc;
    }

    public void MarkDeliveryFailed(string reason)
    {
        if (Status == ShippingStatus.DeliveryFailed)
        {
            return;
        }

        Status = ShippingStatus.DeliveryFailed;
        FailureReason = reason;
    }

    public void MarkFailed(string reason)
    {
        if (Status == ShippingStatus.Failed)
        {
            return;
        }

        Status = ShippingStatus.Failed;
        FailureReason = reason;
    }

    public Result Cancel(string reason)
    {
        if (Status != ShippingStatus.Draft && Status != ShippingStatus.LabelPurchased)
        {
            return ShippingErrors.CannotCancelDispatchedShipment(Id, Status.ToString());
        }

        Status = ShippingStatus.Cancelled;
        FailureReason = reason;
        CancelledAtUtc = DateTime.UtcNow;

        return Result.Ok();
    }

    public void AddTrackingMilestone(
        string status,
        string message,
        string? location,
        DateTime occurredAtUtc)
    {
        var milestone = TrackingMilestone.Create(
            Id,
            status,
            message,
            location,
            occurredAtUtc
        );

        _trackingMilestones.Add(milestone);

        switch (status.ToLowerInvariant())
        {
            case "in_transit" or "transit":
                MarkInTransit(occurredAtUtc);
                break;
            case "out_for_delivery":
                MarkOutForDelivery();
                break;
            case "delivered":
                MarkDelivered(occurredAtUtc);
                break;
            case "failure" or "return_to_sender" or "returned":
                MarkDeliveryFailed(message);
                break;
        }
    }
}
