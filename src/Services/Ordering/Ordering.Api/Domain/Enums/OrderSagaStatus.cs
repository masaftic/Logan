namespace Ordering.Api.Domain.Enums;

public enum OrderSagaStatus
{
    AwaitingPayment = 1,
    AwaitingShipment = 2,
    Completed = 3,
    Failed = 4,
    TimedOut = 5
}
