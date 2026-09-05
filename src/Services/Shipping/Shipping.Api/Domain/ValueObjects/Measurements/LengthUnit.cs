using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[SmartEnum<string>]
public partial class LengthUnit
{
    public static readonly LengthUnit Millimeter = new("mm", "Millimeter", 1.0m);
    public static readonly LengthUnit Centimeter = new("cm", "Centimeter", 10.0m);
    public static readonly LengthUnit Meter = new("m", "Meter", 1000.0m);
    public static readonly LengthUnit Inch = new("in", "Inch", 25.4m);
    public static readonly LengthUnit Foot = new("ft", "Foot", 304.8m);

    public string Name { get; }
    public decimal ConversionFactorToMillimeters { get; }
}
