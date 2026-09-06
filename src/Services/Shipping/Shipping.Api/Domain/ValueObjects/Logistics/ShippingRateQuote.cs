using BuildingBlocks.Common.ValueObjects;
using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ComplexValueObject(DefaultStringComparison = StringComparison.OrdinalIgnoreCase)]
public partial class ShippingRateQuote
{
    public string ProviderRateId { get; }
    public CarrierCode Carrier { get; }
    public string Service { get; }
    public Price Price { get; }
    public CurrencyCode Currency { get; }
    public int? EstDeliveryDays { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref string providerRateId,
        ref CarrierCode carrier,
        ref string service,
        ref Price price,
        ref CurrencyCode currency,
        ref int? estDeliveryDays)
    {
        if (string.IsNullOrWhiteSpace(providerRateId))
        {
            validationError = new ValidationError("ProviderRateId cannot be empty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(service))
        {
            validationError = new ValidationError("Service name cannot be empty.");
        }
    }
}
