using System.Numerics;
using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[ComplexValueObject]
public partial class Length :
    IAdditionOperators<Length, Length, Length>,
    ISubtractionOperators<Length, Length, Length>,
    IComparable<Length>
{
    public decimal Value { get; }
    public LengthUnit Unit { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref decimal value,
        ref LengthUnit unit)
    {
        if (value <= 0)
        {
            validationError = new ValidationError("Length must be greater than zero.");
            return;
        }

        if (unit is null)
        {
            validationError = new ValidationError("Length unit must be specified.");
        }
    }

    public Length To(LengthUnit targetUnit)
    {
        if (Unit == targetUnit)
        {
            return this;
        }

        var mm = Value * Unit.ConversionFactorToMillimeters;
        var targetValue = Math.Round(mm / targetUnit.ConversionFactorToMillimeters, 4);
        return Create(targetValue, targetUnit);
    }

    public Length ToMillimeters() => To(LengthUnit.Millimeter);
    public Length ToCentimeters() => To(LengthUnit.Centimeter);
    public Length ToMeters() => To(LengthUnit.Meter);
    public Length ToInches() => To(LengthUnit.Inch);

    public int CompareTo(Length? other)
    {
        if (other is null)
        {
            return 1;
        }

        var leftMm = Value * Unit.ConversionFactorToMillimeters;
        var rightMm = other.Value * other.Unit.ConversionFactorToMillimeters;
        return leftMm.CompareTo(rightMm);
    }

    public static Length operator +(Length left, Length right)
    {
        var rightConverted = right.To(left.Unit);
        return Create(left.Value + rightConverted.Value, left.Unit);
    }

    public static Length operator -(Length left, Length right)
    {
        var rightConverted = right.To(left.Unit);
        return Create(left.Value - rightConverted.Value, left.Unit);
    }
}
