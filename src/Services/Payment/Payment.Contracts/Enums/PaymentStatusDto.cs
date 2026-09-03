namespace Payment.Contracts.Enums;

public enum PaymentStatusDto
{
    Pending = 1,
    Ready = 2,
    Succeeded = 3,
    PartiallyRefunded = 4,
    Refunded = 5,
    Failed = 6,
    InitializationFailed = 7
}
