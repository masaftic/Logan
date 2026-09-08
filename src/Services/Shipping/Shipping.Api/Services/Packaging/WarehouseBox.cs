using BuildingBlocks.Common.ValueObjects;

namespace Shipping.Api.Services.Packaging;

public record WarehouseBox(
    string Name,
    decimal LengthCm,
    decimal WidthCm,
    decimal HeightCm,
    decimal TareWeightGrams,
    decimal MaxWeightGrams)
{
    public decimal VolumeCm3 => LengthCm * WidthCm * HeightCm;

    public PackageDimensions ToDimensions() =>
        PackageDimensions.Create(LengthCm, WidthCm, HeightCm, LengthUnit.Centimeter);

    public Weight GetTareWeight() =>
        Weight.Create(TareWeightGrams, WeightUnit.Gram);

    public static readonly IReadOnlyList<WarehouseBox> StandardBoxes =
    [
        new("SmallMailer", 22m, 16m, 5m, 50m, 1500m),
        new("MediumBox", 35m, 25m, 12m, 150m, 6000m),
        new("LargeBox", 45m, 35m, 18m, 300m, 15000m),
        new("ExtraLargeBox", 68m, 45m, 15m, 600m, 25000m)
    ];

    public bool CanAccommodate(decimal maxDim1, decimal maxDim2, decimal maxDim3, decimal requiredVolumeCm3, decimal totalItemsWeightGrams)
    {
        // Compare dimensions sorted descending for orientation invariance
        var boxDims = new[] { LengthCm, WidthCm, HeightCm }.OrderByDescending(d => d).ToArray();
        var itemDims = new[] { maxDim1, maxDim2, maxDim3 }.OrderByDescending(d => d).ToArray();

        var fitsDimensions = boxDims[0] >= itemDims[0] &&
                             boxDims[1] >= itemDims[1] &&
                             boxDims[2] >= itemDims[2];

        var fitsVolume = VolumeCm3 >= requiredVolumeCm3;
        var fitsWeight = (totalItemsWeightGrams + TareWeightGrams) <= MaxWeightGrams;

        return fitsDimensions && fitsVolume && fitsWeight;
    }
}
