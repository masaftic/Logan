using BuildingBlocks.Common.ValueObjects;

namespace Shipping.Api.Services.Packaging;

public record PackedParcel(
    PackageDimensions Dimensions,
    Weight Weight,
    string BoxType);
