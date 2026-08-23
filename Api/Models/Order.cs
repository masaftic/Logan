namespace Api.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public decimal Amount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Canceled,
    Failed,
    Timeout
}
