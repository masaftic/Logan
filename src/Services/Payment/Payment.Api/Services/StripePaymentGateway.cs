using BuildingBlocks.Common.Results;
using Microsoft.Extensions.Options;
using Payment.Api.Domain.Errors;
using Stripe;

namespace Payment.Api.Services;

public class StripePaymentGateway : IStripePaymentGateway
{
    private readonly IStripeClient _client;
    private readonly StripeOptions _options;

    public StripePaymentGateway(IOptions<StripeOptions> options)
    {
        _options = options.Value;
        _client = new StripeClient(_options.SecretKey);
    }

    public async Task<Result<StripePaymentIntentResult>> CreatePaymentIntentAsync(
        Guid paymentId,
        Guid orderId,
        decimal amount,
        string currency,
        CancellationToken ct = default)
    {
        try
        {
            var amountInCents = (long)Math.Round(amount * 100m);

            var createOptions = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = currency.ToLowerInvariant(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                },
                Metadata = new Dictionary<string, string>
                {
                    ["payment_id"] = paymentId.ToString(),
                    ["order_id"] = orderId.ToString()
                }
            };

            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"pi:{paymentId}"
            };

            var service = new PaymentIntentService(_client);
            var intent = await service.CreateAsync(createOptions, requestOptions, ct);

            return new StripePaymentIntentResult(intent.Id, intent.ClientSecret, intent.Status);
        }
        catch (StripeException ex)
        {
            return PaymentErrors.StripeError(ex.Message);
        }
    }

    public async Task<Result<string>> ConfirmTestPaymentAsync(
        string paymentIntentId,
        string paymentMethod,
        CancellationToken ct = default)
    {
        try
        {
            var service = new PaymentIntentService(_client);
            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethod
            };

            var intent = await service.ConfirmAsync(paymentIntentId, confirmOptions, cancellationToken: ct);
            return intent.Status;
        }
        catch (StripeException ex)
        {
            return PaymentErrors.StripeError(ex.Message);
        }
    }

    public async Task<Result<string>> RefundAsync(
        Guid refundId,
        string paymentIntentId,
        decimal amount,
        string reason,
        CancellationToken ct = default)
    {
        try
        {
            var amountInCents = (long)Math.Round(amount * 100m);

            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = amountInCents,
                Reason = RefundReasons.RequestedByCustomer,
                Metadata = new Dictionary<string, string>
                {
                    ["refund_id"] = refundId.ToString(),
                    ["reason"] = reason
                }
            };

            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"refund:{refundId}"
            };

            var refundService = new RefundService(_client);
            var refund = await refundService.CreateAsync(refundOptions, requestOptions, ct);

            return refund.Id;
        }
        catch (StripeException ex)
        {
            return PaymentErrors.StripeError(ex.Message);
        }
    }

    public Result<Event> ConstructWebhookEvent(
        string jsonPayload,
        string signatureHeader)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                jsonPayload,
                signatureHeader,
                _options.WebhookSecret,
                throwOnApiVersionMismatch: false
            );

            return stripeEvent;
        }
        catch (Exception)
        {
            return PaymentErrors.InvalidWebhookSignature();
        }
    }
}
