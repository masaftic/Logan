using Thinktecture;

namespace BuildingBlocks.Common.ValueObjects;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public partial class CurrencyCode
{
    public static readonly CurrencyCode EUR = Create("EUR");
    public static readonly CurrencyCode USD = Create("USD");

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 3)
        {
            validationError = new ValidationError("Currency code must be 3 characters long.");
            return;
        }

        value = value.ToUpperInvariant();
    }
}
