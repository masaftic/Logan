namespace Payment.Contracts.DTOs;

public record PaymentRefundDto(
    Guid Id,
    decimal Amount,
    string Reason,
    string ProviderRefundId,
    DateTime CreatedAtUtc
);
