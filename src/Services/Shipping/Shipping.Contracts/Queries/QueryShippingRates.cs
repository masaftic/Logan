using Shipping.Contracts.DTOs;

namespace Shipping.Contracts.Queries;

public record QueryShippingRates(
    ShippingAddressDto DestinationAddress,
    PackageDimensionsDto Dimensions,
    PackageWeightDto Weight);

