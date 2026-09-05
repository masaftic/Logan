namespace Shipping.Api.Domain.Enums;

public enum ShippingStatus
{
    Draft = 1,
    LabelPurchased = 2,
    InTransit = 3,
    OutForDelivery = 4,
    Delivered = 5,
    DeliveryFailed = 6,
    Cancelled = 7,
    Failed = 8
}
