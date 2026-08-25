namespace Contracts.Payments.DTOs;

public record PaymentIntentResponse(
    string PaymentIntentId,
    string ClientSecret,
    Guid OrderId,
    decimal Amount,
    string Status
);

public record PaymentResponse(
    Guid Id,
    decimal Amount,
    string Status,
    string? GatewayTransactionId,
    string? FailureReason,
    DateTime CreatedAtUtc,
    DateTime? CompletedAtUtc
);
