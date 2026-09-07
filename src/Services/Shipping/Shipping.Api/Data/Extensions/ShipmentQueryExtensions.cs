using Microsoft.EntityFrameworkCore;
using Shipping.Api.Domain;
using Shipping.Api.Domain.Enums;

namespace Shipping.Api.Data.Extensions;

public static class ShipmentQueryExtensions
{
    public static IQueryable<Shipment> WhereActiveOrPurchased(
        this IQueryable<Shipment> query)
        => query.Where(s => s.Status == ShippingStatus.Draft ||
                            s.Status == ShippingStatus.LabelPurchased ||
                            s.Status == ShippingStatus.InTransit);

    public static IQueryable<Shipment> IncludeShipmentDetails(
        this IQueryable<Shipment> query)
        => query.Include(s => s.Items)
                .Include(s => s.TrackingMilestones);
}
