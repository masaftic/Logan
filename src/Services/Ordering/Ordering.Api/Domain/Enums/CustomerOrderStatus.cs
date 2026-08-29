namespace Ordering.Api.Domain.Enums;

public enum CustomerOrderStatus
{
    Processing = 1,
    PaymentPending = 2,
    PreparingForDispatch = 3,
    Shipped = 4,
    Completed = 5,
    Cancelled = 6,
    Refunded = 7
}
