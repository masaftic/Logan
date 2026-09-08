using Thinktecture;

namespace BuildingBlocks.Common.ValueObjects;

[ComplexValueObject]
public partial class PackageDimensions
{
    public Length Length { get; }
    public Length Width { get; }
    public Length Height { get; }

    private PackageDimensions()
    {
        Length = default!;
        Width = default!;
        Height = default!;
    }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length length,
        ref Length width,
        ref Length height)
    {
        if (length is null || width is null || height is null)
        {
            validationError = new ValidationError("Length, width, and height must all be provided.");
        }
    }

    public static PackageDimensions Create(decimal length, decimal width, decimal height, LengthUnit unit)
    {
        return Create(
            Length.Create(length, unit),
            Length.Create(width, unit),
            Length.Create(height, unit)
        );
    }

    public PackageDimensions To(LengthUnit targetUnit)
    {
        return Create(
            Length.To(targetUnit), 
            Width.To(targetUnit), 
            Height.To(targetUnit)
        );
    }

    public decimal VolumeInCubicCentimeters =>
        Length.ToCentimeters().Value * Width.ToCentimeters().Value * Height.ToCentimeters().Value;
}
