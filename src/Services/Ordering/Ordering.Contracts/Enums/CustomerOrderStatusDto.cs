namespace Ordering.Contracts.Enums;

public enum CustomerOrderStatusDto
{
    Processing = 1,
    PaymentPending = 2,
    PreparingForDispatch = 3,
    Shipped = 4,
    Completed = 5,
    Cancelled = 6,
    Refunded = 7
}
