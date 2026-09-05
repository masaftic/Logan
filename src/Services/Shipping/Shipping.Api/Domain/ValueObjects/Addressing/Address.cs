using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ComplexValueObject(DefaultStringComparison = StringComparison.OrdinalIgnoreCase)]
public partial class Address
{
    public Street Street { get; }
    public string? SecondaryStreet { get; }
    public City City { get; }
    public StateOrProvince State { get; }
    public PostalCode PostalCode { get; }
    public CountryCode Country { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Street street,
        ref string? secondaryStreet,
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
