using Thinktecture;

namespace Inventory.Api.Domain.ValueObjects;

[ValueObject<int>]
public readonly partial struct Quantity
{
    public static readonly Quantity Zero = (Quantity)0;

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value)
    {
        if (value < 0)
        {
            validationError = new ValidationError("Quantity cannot be negative.");
        }
    }
}

