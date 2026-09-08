using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Catalog.Contracts.DTOs;
using Shipping.Api.Services.Clients;
using Shipping.Contracts.DTOs;

namespace Shipping.Api.Services.Packaging;

public class CatalogPackagingStrategy : IPackagingStrategy
{
    private const decimal DunnageVolumeMultiplier = 1.20m; // 20% volume padding for dunnage/cushioning
    private readonly ICatalogClient _catalogClient;

    public CatalogPackagingStrategy(ICatalogClient _catalogClient)
    {
        this._catalogClient = _catalogClient;
    }

    public async Task<Result<PackedParcel>> CalculatePackageAsync(
        IReadOnlyList<ShipmentItemDto> items,
        CancellationToken cancellationToken = default)
    {
        if (items is null || items.Count == 0)
        {
            return Error.Validation("Packaging.EmptyItems", "Cannot calculate packaging for an empty item list.");
        }

        if (items.Any(i => i.Quantity <= 0))
        {
            return Error.Validation("Packaging.InvalidQuantity", "Item quantities must be greater than zero.");
        }

        var uniqueSkus = items.Select(i => i.Sku.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var productsResult = await _catalogClient.GetProductsBySkusAsync(uniqueSkus, cancellationToken);
        if (productsResult.IsError)
        {
            return productsResult.Errors;
        }

        var productCatalog = productsResult.Value.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);

        foreach (var sku in uniqueSkus)
        {
            if (!productCatalog.ContainsKey(sku))
            {
                return Error.NotFound("Packaging.ProductNotFound", $"Product with SKU '{sku}' was not found in catalog.");
            }
        }

        decimal totalItemsWeightGrams = 0m;
        decimal totalItemsVolumeCm3 = 0m;
        decimal maxDim1 = 0m;
        decimal maxDim2 = 0m;
        decimal maxDim3 = 0m;

        foreach (var item in items)
        {
            var product = productCatalog[item.Sku.Trim()];

            var weightUnit = WeightUnit.TryGet(product.WeightUnit, out var wu) ? wu : WeightUnit.Gram;
            var itemWeightGrams = Weight.Create(product.WeightValue, weightUnit).ToGrams().Value;

            var lengthUnit = LengthUnit.TryGet(product.DimensionUnit, out var lu) ? lu : LengthUnit.Centimeter;
            var lengthCm = Length.Create(product.Length, lengthUnit).ToCentimeters().Value;
            var widthCm = Length.Create(product.Width, lengthUnit).ToCentimeters().Value;
            var heightCm = Length.Create(product.Height, lengthUnit).ToCentimeters().Value;

            var itemDims = new[] { lengthCm, widthCm, heightCm }.OrderByDescending(d => d).ToArray();
            maxDim1 = Math.Max(maxDim1, itemDims[0]);
            maxDim2 = Math.Max(maxDim2, itemDims[1]);
            maxDim3 = Math.Max(maxDim3, itemDims[2]);

            var itemVolumeCm3 = lengthCm * widthCm * heightCm;

            totalItemsWeightGrams += itemWeightGrams * item.Quantity;
            totalItemsVolumeCm3 += itemVolumeCm3 * item.Quantity;
        }

        var requiredVolumeCm3 = totalItemsVolumeCm3 * DunnageVolumeMultiplier;

        var selectedBox = WarehouseBox.StandardBoxes
            .OrderBy(b => b.VolumeCm3)
            .FirstOrDefault(b => b.CanAccommodate(maxDim1, maxDim2, maxDim3, requiredVolumeCm3, totalItemsWeightGrams));

        if (selectedBox is not null)
        {
            var finalWeightGrams = totalItemsWeightGrams + selectedBox.TareWeightGrams;
            var dimensions = selectedBox.ToDimensions();
            var weight = Weight.Create(finalWeightGrams, WeightUnit.Gram);

            return new PackedParcel(dimensions, weight, selectedBox.Name);
        }

        // Oversized custom packaging fallback
        var customLength = Math.Max(Math.Round(maxDim1 * 1.10m, 1), 10m);
        var customWidth = Math.Max(Math.Round(maxDim2 * 1.10m, 1), 10m);
        var customHeight = Math.Max(Math.Round(maxDim3 * 1.10m, 1), 5m);
        const decimal customTareWeightGrams = 800m;

        var customDimensions = PackageDimensions.Create(customLength, customWidth, customHeight, LengthUnit.Centimeter);
        var customWeight = Weight.Create(totalItemsWeightGrams + customTareWeightGrams, WeightUnit.Gram);

        return new PackedParcel(customDimensions, customWeight, "CustomOversizedBox");
    }
}
