using System;
using System.Numerics;
using Thinktecture;

namespace BuildingBlocks.Common.ValueObjects;

[ValueObject<decimal>(
   DefaultInstancePropertyName = "Zero",
   AllowDefaultStructs = true,
   MultiplyOperators = OperatorsGeneration.None,
   DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Price
   : IMultiplyOperators<Price, int, Price> // Multiplication with int don't lead to more than 2 decimal places
{
    static partial void ValidateFactoryArguments(
         ref ValidationError? validationError,
         ref decimal value,
         PriceRoundingStrategy? roundingStrategy)
    {
        if (value < 0)
        {
            validationError = new ValidationError("Amount cannot be negative");
            return;
        }

        value = (roundingStrategy ?? PriceRoundingStrategy.Default).Round(value);
    }

    public static Price? Create(decimal? amount, PriceRoundingStrategy roundingStrategy)
    {
        return amount is null ? null : CreateCore(amount.Value, roundingStrategy);
    }

    public static Price Create(decimal amount, PriceRoundingStrategy roundingStrategy)
    {
        return CreateCore(amount, roundingStrategy);
    }

    public static Price operator *(Price left, int right)
    {
        return Create(left._value * right);
    }

    public static Price operator *(int right, Price left)
    {
        return Create(left._value * right);
    }
}

[SmartEnum]
public partial class PriceRoundingStrategy
{
    public static readonly PriceRoundingStrategy Default = new(d => decimal.Round(d, 2));
    public static readonly PriceRoundingStrategy Up = new(d => decimal.Round(d, 2, MidpointRounding.ToPositiveInfinity));
    public static readonly PriceRoundingStrategy Down = new(d => decimal.Round(d, 2, MidpointRounding.ToNegativeInfinity));

    [UseDelegateFromConstructor]
    public partial decimal Round(decimal value);
}
