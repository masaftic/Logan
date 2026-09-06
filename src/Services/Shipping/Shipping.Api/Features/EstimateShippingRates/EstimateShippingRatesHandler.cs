using BuildingBlocks.Common.Results;
using Microsoft.Extensions.Options;
using Shipping.Api.Domain.Errors;
using Shipping.Api.Domain.ValueObjects;
using Shipping.Api.Services;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Queries;

namespace Shipping.Api.Features.EstimateShippingRates;

public class EstimateShippingRatesHandler
{
    private readonly IShippingGateway _shippingGateway;
    private readonly WarehouseOptions _warehouseOptions;

    public EstimateShippingRatesHandler(
        IShippingGateway shippingGateway,
        IOptions<WarehouseOptions> warehouseOptions)
    {
        _shippingGateway = shippingGateway;
        _warehouseOptions = warehouseOptions.Value;
    }

    public async Task<Result<IReadOnlyList<ShippingRateQuoteDto>>> Handle(
        QueryShippingRates query,
        CancellationToken cancellationToken)
    {
        var destCountry = CountryCode.TryGet(query.DestinationAddress.Country, out var country)
            ? country
            : CountryCode.US;

        var destinationAddress = Address.Create(
            Street.Create(query.DestinationAddress.Street1),
            query.DestinationAddress.Street2 is not null ? Street.Create(query.DestinationAddress.Street2) : null,
            City.Create(query.DestinationAddress.City),
            StateOrProvince.Create(query.DestinationAddress.State),
            PostalCode.Create(query.DestinationAddress.PostalCode),
            destCountry);

        var originAddress = Address.Create(
            Street.Create(_warehouseOptions.Street1),
            null,
            City.Create(_warehouseOptions.City),
            StateOrProvince.Create(_warehouseOptions.State),
            PostalCode.Create(_warehouseOptions.Zip),
            CountryCode.US);


        var lengthUnit = LengthUnit.TryGet(query.Dimensions.Unit, out var lUnit)
            ? lUnit
            : LengthUnit.Centimeter;

        var dimensions = PackageDimensions.Create(
            query.Dimensions.Length,
            query.Dimensions.Width,
            query.Dimensions.Height,
            lengthUnit);

        var weightUnit = WeightUnit.TryGet(query.Weight.Unit, out var wUnit)
            ? wUnit
            : WeightUnit.Gram;

        var weight = Weight.Create(query.Weight.Value, weightUnit);

        var ratesResult = await _shippingGateway.GetRatesAsync(
            originAddress,
            destinationAddress,
            dimensions,
            weight,
            cancellationToken);

        if (ratesResult.IsError)
        {
            return ratesResult.Errors;
        }

        var dtos = ratesResult.Value.Select(quote => new ShippingRateQuoteDto(
            ProviderRateId: quote.ProviderRateId,
            Carrier: quote.Carrier,
            Service: quote.Service,
            Amount: quote.Price,
            Currency: quote.Currency,
            EstDeliveryDays: quote.EstDeliveryDays,
            DurationTerms: null
        )).ToList();

        return dtos.AsReadOnly();
    }
}
