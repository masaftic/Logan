using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ComplexValueObject(DefaultStringComparison = StringComparison.OrdinalIgnoreCase)]
public partial class Address
{
    public Street Street1 { get; }
    public Street? Street2 { get; }
    public City City { get; }
    public StateOrProvince State { get; }
    public PostalCode PostalCode { get; }
    public CountryCode Country { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Street street1,
        ref Street? street2,
        ref City city,
        ref StateOrProvince state,
        ref PostalCode postalCode,
        ref CountryCode country)
    {
        if (country.PostalCodeLength > 0 && postalCode.Length != country.PostalCodeLength)
        {
            validationError = new ValidationError(
                $"Postal code length for country {country} must be {country.PostalCodeLength}.");
        }
    }
}
