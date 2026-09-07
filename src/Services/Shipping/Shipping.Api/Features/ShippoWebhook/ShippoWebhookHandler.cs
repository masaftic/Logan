using Microsoft.EntityFrameworkCore;
using Shipping.Api.Data;
using Shipping.Api.Domain.Enums;
using Shipping.Api.Domain.ValueObjects;
using Shipping.Contracts.Commands;
using Shipping.Contracts.Events;
using Wolverine;

namespace Shipping.Api.Features.ShippoWebhook;

public static class ShippoWebhookHandler
{
    public static async Task<object?> Handle(
        CommandProcessShippoWebhook command,
        ShippingDbContext dbContext,
        IMessageBus bus,
        CancellationToken ct)
    {
        if (!TrackingNumber.TryCreate(command.TrackingNumber, out var trackingNumber))
        {
            return null;
        }

        var shipment = await dbContext.Shipments
            .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber, ct);

        if (shipment is null)
        {
            return null;
        }

        var previousStatus = shipment.Status;
        var occurredAt = command.OccurredAtUtc ?? DateTime.UtcNow;
        var message = command.Message ?? command.Status;

        shipment.AddTrackingMilestone(command.Status, message, command.Location, occurredAt);

        if (shipment.Status != previousStatus)
        {
            switch (shipment.Status)
            {
                case ShippingStatus.InTransit:
                    return new ShipmentInTransitEvent(
                        shipment.Id,
                        shipment.OrderId,
                        shipment.TrackingNumber!,
                        command.Location,
                        occurredAt);

                case ShippingStatus.Delivered:
                    return new ShipmentDeliveredEvent(
                        shipment.Id,
                        shipment.OrderId,
                        shipment.TrackingNumber!,
                        occurredAt);

                case ShippingStatus.DeliveryFailed:
                    return new ShipmentDeliveryFailedEvent(
                        shipment.Id,
                        shipment.OrderId,
                        shipment.TrackingNumber!,
                        message,
                        occurredAt);
            }
        }

        return null;
    }
}
