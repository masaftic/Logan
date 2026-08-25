namespace Api.Domain.Payments;

public enum PaymentStatus
{
    Initiated,
    Processing,
    Success,
    Failed,
    Refunded
}
