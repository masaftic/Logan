using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[SmartEnum<string>]
public partial class WeightUnit
{
    public static readonly WeightUnit Gram = new("g", "Gram", 1.0m);
    public static readonly WeightUnit Kilogram = new("kg", "Kilogram", 1000.0m);
    public static readonly WeightUnit Ounce = new("oz", "Ounce", 28.349523125m);
    public static readonly WeightUnit Pound = new("lb", "Pound", 453.59237m);

    public string Name { get; }
    public decimal ConversionFactorToGrams { get; }
}
