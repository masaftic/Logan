using Payment.Contracts.Enums;

namespace Payment.Contracts.DTOs;

public record PaymentInitializationDto(
    Guid OrderId,
    PaymentStatusDto Status,
    string? ClientSecret,
    string? PaymentIntentId,
    decimal Amount,
    string Currency,
    string? ErrorMessage = null
);
