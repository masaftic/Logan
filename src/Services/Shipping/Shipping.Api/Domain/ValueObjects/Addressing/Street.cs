using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public partial class Street
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Street cannot be empty.");
            return;
        }

        value = value.Trim();

        if (value.Length > 200)
        {
            validationError = new ValidationError("Street cannot exceed 200 characters.");
        }
    }
}
