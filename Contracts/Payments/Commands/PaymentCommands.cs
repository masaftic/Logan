namespace Contracts.Payments.Commands;

public record CreatePaymentIntentCommand(Guid OrderId);

public record ProcessPaymentCommand(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string IdempotencyKey
);

public record RefundPaymentCommand(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string Reason
);

public record ProcessPaymentWebhookCommand(
    string EventType,
    Guid OrderId,
    string GatewayTransactionId,
    decimal Amount,
    string? FailureReason = null
);
