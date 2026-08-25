namespace Api.Features.Payments.Services;

public interface IPaymentGateway
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        Guid orderId, 
        decimal amount, 
        CancellationToken ct = default);

    Task<PaymentExecutionResult> ProcessPaymentAsync(
        PaymentRequest request, 
        CancellationToken ct = default);

    Task<PaymentStatusResult> GetStatusAsync(
        string idempotencyKey, 
        CancellationToken ct = default);

    Task<RefundResult> RefundPaymentAsync(
        RefundRequest request, 
        CancellationToken ct = default);
}

public record PaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret,
    Guid OrderId,
    decimal Amount
);

public record PaymentRequest(
    Guid OrderId,
    decimal Amount,
    string IdempotencyKey
);

public record PaymentExecutionResult(
    bool IsSuccess,
    Guid? TransactionId,
    decimal Amount,
    string? FailureReason,
    GatewayTransactionStatus Status
);

public record PaymentStatusResult(
    bool Exists,
    Guid? TransactionId,
    decimal? Amount,
    GatewayTransactionStatus Status
);

public record RefundRequest(
    string TransactionId,
    decimal Amount,
    string Reason
);

public record RefundResult(
    bool IsSuccess,
    Guid? RefundId,
    string? ErrorMessage
);

public enum GatewayTransactionStatus
{
    Success,
    Declined,
    Refunded,
    NotFound
}
