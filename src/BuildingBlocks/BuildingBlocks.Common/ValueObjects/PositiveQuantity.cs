using Thinktecture;

namespace BuildingBlocks.Common.ValueObjects;

[ValueObject<int>]
public readonly partial struct PositiveQuantity
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value)
    {
        if (value <= 0)
        {
            validationError = new ValidationError("Quantity must be greater than zero.");
        }
    }

    public static implicit operator Quantity(PositiveQuantity positiveQuantity) => Quantity.Create((int)positiveQuantity);
}

