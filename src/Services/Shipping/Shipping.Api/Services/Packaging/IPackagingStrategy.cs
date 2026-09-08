using BuildingBlocks.Common.Results;
using Shipping.Contracts.DTOs;

namespace Shipping.Api.Services.Packaging;

public interface IPackagingStrategy
{
    Task<Result<PackedParcel>> CalculatePackageAsync(
        IReadOnlyList<ShipmentItemDto> items,
        CancellationToken cancellationToken = default);
}
