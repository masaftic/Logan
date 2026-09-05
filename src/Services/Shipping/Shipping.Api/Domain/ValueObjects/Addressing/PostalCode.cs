using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public partial class PostalCode
{
    public int Length => _value.Length;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Postal code cannot be empty.");
            return;
        }

        value = value.Trim().ToUpperInvariant();

        if (value.Length < 3 || value.Length > 12)
        {
            validationError = new ValidationError("Postal code must be between 3 and 12 characters.");
        }
    }
}
