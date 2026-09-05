using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public partial class TrackingNumber
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Tracking number cannot be empty.");
            return;
        }

        value = value.Trim().ToUpperInvariant();
    }
}
