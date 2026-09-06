namespace Shipping.Api.Services;

public class ShippoOptions
{
    public const string SectionName = "Shippo";

    public string ApiKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}
