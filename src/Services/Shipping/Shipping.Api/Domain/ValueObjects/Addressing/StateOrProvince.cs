using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public partial class StateOrProvince
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("State or province cannot be empty.");
            return;
        }

        value = value.Trim().ToUpperInvariant();

        if (value.Length > 50)
        {
            validationError = new ValidationError("State or province cannot exceed 50 characters.");
        }
    }
}
