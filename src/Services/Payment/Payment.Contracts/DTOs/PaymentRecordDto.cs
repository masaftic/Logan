using Payment.Contracts.Enums;

namespace Payment.Contracts.DTOs;

public record PaymentRecordDto(
    Guid Id,
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    decimal AmountRefunded,
    decimal RemainingAmount,
    string Currency,
    PaymentStatusDto Status,
    string? PaymentIntentId,
    string? ClientSecret,
    IReadOnlyList<PaymentRefundDto> Refunds,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? FailedAtUtc,
    string? FailureReason
);
