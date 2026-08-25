namespace Contracts.Payments.DTOs;

public record PaymentIntentResponse(
    string PaymentIntentId,
    string ClientSecret,
    Guid OrderId,
    decimal Amount,
    string Status
);

public record PaymentWebhookRequest(
    string EventType,
    Guid OrderId,
    string GatewayTransactionId,
    decimal Amount,
    string? FailureReason = null
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
