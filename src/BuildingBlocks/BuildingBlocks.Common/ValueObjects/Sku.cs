using System;
using Thinktecture;

namespace BuildingBlocks.Common.ValueObjects;

[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public partial class Sku
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Sku cannot be empty.");
        }

        value = value.Trim().ToUpperInvariant();
    }
}
