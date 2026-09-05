namespace Shipping.Api.Domain;

public class TrackingMilestone
{
    public long Id { get; private set; }
    public ShipmentId ShipmentId { get; private set; }
    public string Status { get; private set; } = null!;
    public string Message { get; private set; } = null!;
    public string? Location { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private TrackingMilestone() { }

    public static TrackingMilestone Create(
        ShipmentId shipmentId,
        string status,
        string message,
        string? location,
        DateTime occurredAtUtc)
    {
        return new TrackingMilestone
        {
            ShipmentId = shipmentId,
            Status = status,
            Message = message,
            Location = location,
            OccurredAtUtc = occurredAtUtc
        };
    }
}
