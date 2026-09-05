using System.Numerics;
using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ComplexValueObject]
public partial class Weight :
    IAdditionOperators<Weight, Weight, Weight>,
    ISubtractionOperators<Weight, Weight, Weight>,
    IComparable<Weight>
{
    public decimal Value { get; }
    public WeightUnit Unit { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref decimal value,
        ref WeightUnit unit)
    {
        if (value <= 0)
        {
            validationError = new ValidationError("Weight must be greater than zero.");
            return;
        }

        if (unit is null)
        {
            validationError = new ValidationError("Weight unit must be specified.");
        }
    }

    public Weight To(WeightUnit targetUnit)
    {
        if (Unit == targetUnit)
        {
            return this;
        }

        var grams = Value * Unit.ConversionFactorToGrams;
        var targetValue = Math.Round(grams / targetUnit.ConversionFactorToGrams, 4);
        return Create(targetValue, targetUnit);
    }

    public Weight ToGrams() => To(WeightUnit.Gram);
    public Weight ToKilograms() => To(WeightUnit.Kilogram);
    public Weight ToOunces() => To(WeightUnit.Ounce);
    public Weight ToPounds() => To(WeightUnit.Pound);

    public int CompareTo(Weight? other)
    {
        if (other is null)
        {
            return 1;
        }

        var leftGrams = Value * Unit.ConversionFactorToGrams;
        var rightGrams = other.Value * other.Unit.ConversionFactorToGrams;
        return leftGrams.CompareTo(rightGrams);
    }

    public static Weight operator +(Weight left, Weight right)
    {
        var rightConverted = right.To(left.Unit);
        return Create(left.Value + rightConverted.Value, left.Unit);
    }

    public static Weight operator -(Weight left, Weight right)
    {
        var rightConverted = right.To(left.Unit);
        return Create(left.Value - rightConverted.Value, left.Unit);
    }
}
