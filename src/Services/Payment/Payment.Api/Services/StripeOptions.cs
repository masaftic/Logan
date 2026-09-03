namespace Payment.Api.Services;

public class StripeOptions
{
    public const string SectionName = "Stripe";

    public string SecretKey { get; set; } = "sk_test_placeholder";
    public string PublishableKey { get; set; } = "pk_test_placeholder";
    public string WebhookSecret { get; set; } = "whsec_placeholder";
}
