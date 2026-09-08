using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Shipping.Api.Domain.ValueObjects;

namespace Shipping.Api.Services;

public interface IShippingGateway
{
    Task<Result<IReadOnlyList<ShippingRateQuote>>> GetRatesAsync(
        Address originAddress,
        Address destinationAddress,
        PackageDimensions dimensions,
        Weight weight,
        CancellationToken cancellationToken = default);

    Task<Result<ShippingRateQuote>> GetRateByIdAsync(
        string providerRateId,
        CancellationToken cancellationToken = default);

    Task<Result<PurchasedShippingLabel>> PurchaseLabelAsync(
        string providerRateId,
        CancellationToken cancellationToken = default);
}

public record PurchasedShippingLabel(
    string ProviderTransactionId,
    string TrackingNumber,
    string LabelUrl,
    string? ProviderTrackerId);

