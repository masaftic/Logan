namespace Api.Domain.Orders;

public enum OrderStatus
{
    Pending,
    Processing,
    Completed,
    Canceled,
    Failed,
    TimedOut
}
