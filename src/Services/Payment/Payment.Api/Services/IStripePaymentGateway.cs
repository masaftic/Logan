using BuildingBlocks.Common.Results;

namespace Payment.Api.Services;

public interface IStripePaymentGateway
{
    Task<Result<StripePaymentIntentResult>> CreatePaymentIntentAsync(
        Guid paymentId,
        Guid orderId,
        decimal amount,
        string currency,
        CancellationToken ct = default);

    Task<Result<string>> ConfirmTestPaymentAsync(
        string paymentIntentId,
        string paymentMethod,
        CancellationToken ct = default);

    Task<Result<string>> RefundAsync(
        Guid refundId,
        string paymentIntentId,
        decimal amount,
        string reason,
        CancellationToken ct = default);

    Result<Stripe.Event> ConstructWebhookEvent(
        string jsonPayload,
        string signatureHeader);
}

public record StripePaymentIntentResult(
    string PaymentIntentId,
    string ClientSecret,
    string Status
);
