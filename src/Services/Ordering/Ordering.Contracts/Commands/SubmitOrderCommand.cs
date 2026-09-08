using Ordering.Contracts.DTOs;
using Shipping.Contracts.DTOs;

namespace Ordering.Contracts.Commands;

public record SubmitOrderCommand(
    Guid CustomerId,
    IReadOnlyList<OrderItemRequestDto> Items,
    string Currency,
    string ProviderRateId,
    ShippingAddressDto DestinationAddress,
    PackageDimensionsDto Dimensions,
    PackageWeightDto Weight
);

