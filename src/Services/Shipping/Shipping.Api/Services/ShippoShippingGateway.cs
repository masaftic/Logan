using System.Globalization;
using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shippo;
using Shippo.Models.Components;
using Shippo.Models.Requests;
using Shipping.Api.Domain.Errors;
using Shipping.Api.Domain.ValueObjects;

namespace Shipping.Api.Services;

public class ShippoShippingGateway : IShippingGateway
{
    private readonly ShippoSDK _shippoSdk;
    private readonly WarehouseOptions _warehouseOptions;
    private readonly ILogger<ShippoShippingGateway> _logger;

    public ShippoShippingGateway(
        ShippoSDK shippoSdk,
        IOptions<WarehouseOptions> warehouseOptions,
        ILogger<ShippoShippingGateway> logger)
    {
        _shippoSdk = shippoSdk;
        _warehouseOptions = warehouseOptions.Value;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<ShippingRateQuote>>> GetRatesAsync(
        Domain.ValueObjects.Address originAddress,
        Domain.ValueObjects.Address destinationAddress,
        PackageDimensions dimensions,
        Weight weight,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var addressFrom = AddressFrom.CreateAddressCreateRequest(new AddressCreateRequest
            {
                Name = _warehouseOptions.Name,
                Street1 = originAddress.Street1,
                Street2 = originAddress.Street2,
                City = originAddress.City,
                State = originAddress.State,
                Zip = originAddress.PostalCode,
                Country = originAddress.Country.Key,
                Phone = _warehouseOptions.Phone,
                Email = _warehouseOptions.Email
            });

            var addressTo = AddressTo.CreateAddressCreateRequest(new AddressCreateRequest
            {
                Name = "Customer",
                Street1 = destinationAddress.Street1,
                Street2 = destinationAddress.Street2,
                City = destinationAddress.City,
                State = destinationAddress.State,
                Zip = destinationAddress.PostalCode,
                Country = destinationAddress.Country.Key
            });

            var distanceUnit = dimensions.Length.Unit.Key switch
            {
                "cm" => DistanceUnitEnum.Cm,
                "mm" => DistanceUnitEnum.Mm,
                "m" => DistanceUnitEnum.M,
                "in" => DistanceUnitEnum.In,
                "ft" => DistanceUnitEnum.In,
                _ => DistanceUnitEnum.Cm
            };

            var normalizedDimensions = dimensions.Length.Unit.Key == "ft"
                ? dimensions.To(LengthUnit.Inch)
                : dimensions;

            var massUnit = weight.Unit.Key switch
            {
                "g" => WeightUnitEnum.G,
                "kg" => WeightUnitEnum.Kg,
                "oz" => WeightUnitEnum.Oz,
                "lb" => WeightUnitEnum.Lb,
                _ => WeightUnitEnum.G
            };

            var parcel = Shippo.Models.Components.Parcels.CreateParcelCreateRequest(new ParcelCreateRequest
            {
                Length = normalizedDimensions.Length.Value.ToString(CultureInfo.InvariantCulture),
                Width = normalizedDimensions.Width.Value.ToString(CultureInfo.InvariantCulture),
                Height = normalizedDimensions.Height.Value.ToString(CultureInfo.InvariantCulture),
                DistanceUnit = distanceUnit,
                Weight = weight.Value.ToString(CultureInfo.InvariantCulture),
                MassUnit = massUnit
            });

            var request = new ShipmentCreateRequest
            {
                AddressFrom = addressFrom,
                AddressTo = addressTo,
                Parcels = [parcel],
                Async = false
            };

            _logger.LogInformation(
                "Requesting Shippo shipping rates to {City}, {State}, {Country}",
                destinationAddress.City,
                destinationAddress.State,
                destinationAddress.Country.Key);

            var shipment = await _shippoSdk.Shipments.CreateAsync(request);

            if (shipment is null)
            {
                return ShippingErrors.ShippoError("No response received from Shippo shipping API.");
            }

            var quotes = new List<ShippingRateQuote>();

            if (shipment.Rates is not null)
            {
                foreach (var rate in shipment.Rates)
                {
                    var quote = MapRateToQuote(rate);
                    if (quote is not null)
                    {
                        quotes.Add(quote);
                    }
                }
            }

            if (quotes.Count == 0 && shipment.Messages is not null && shipment.Messages.Count > 0)
            {
                _logger.LogWarning("Shippo returned messages: {@Messages}", shipment.Messages);
                var errorMessages = string.Join("; ", shipment.Messages.Select(m => m.Text));
                return ShippingErrors.ShippoError(errorMessages);
            }

            _logger.LogInformation("Successfully retrieved {Count} shipping rates from Shippo", quotes.Count);
            return quotes.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error communicating with Shippo API");
            return ShippingErrors.ShippoError(ex.Message);
        }
    }

    public async Task<Result<ShippingRateQuote>> GetRateByIdAsync(
        string providerRateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Retrieving Shippo rate {RateId}", providerRateId);

            var rate = await _shippoSdk.Rates.GetAsync(providerRateId);
            if (rate is null)
            {
                return ShippingErrors.ShippoError($"Rate {providerRateId} not found in Shippo.");
            }

            var quote = MapRateToQuote(rate);
            if (quote is null)
            {
                return ShippingErrors.ShippoError($"Rate {providerRateId} contains invalid rate details.");
            }

            return quote;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving rate {RateId} from Shippo", providerRateId);
            return ShippingErrors.ShippoError(ex.Message);
        }
    }

    public async Task<Result<PurchasedShippingLabel>> PurchaseLabelAsync(
        string providerRateId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Purchasing shipping label for Shippo rate {RateId}", providerRateId);

            var request = CreateTransactionRequestBody.CreateTransactionCreateRequest(new TransactionCreateRequest
            {
                Rate = providerRateId,
                LabelFileType = LabelFileTypeEnum.Pdf,
                Async = false
            });

            var transaction = await _shippoSdk.Transactions.CreateAsync(request);

            if (transaction is null)
            {
                return ShippingErrors.ShippoError("No response received from Shippo transactions API.");
            }

            if (transaction.Status != TransactionStatusEnum.Success)
            {
                var errorMessages = transaction.Messages is not null && transaction.Messages.Count > 0
                    ? string.Join("; ", transaction.Messages.Select(m => m.Text))
                    : $"Transaction failed with status {transaction.Status}.";

                _logger.LogWarning("Shippo transaction failed for rate {RateId}: {Error}", providerRateId, errorMessages);
                return ShippingErrors.ShippoError(errorMessages);
            }

            if (string.IsNullOrWhiteSpace(transaction.ObjectId) ||
                string.IsNullOrWhiteSpace(transaction.TrackingNumber) ||
                string.IsNullOrWhiteSpace(transaction.LabelUrl))
            {
                return ShippingErrors.ShippoError("Shippo transaction succeeded but transaction ID, tracking number, or label URL was empty.");
            }

            _logger.LogInformation(
                "Successfully purchased label: TrackingNumber={TrackingNumber}, TransactionId={TransactionId}",
                transaction.TrackingNumber,
                transaction.ObjectId);

            return new PurchasedShippingLabel(
                ProviderTransactionId: transaction.ObjectId,
                TrackingNumber: transaction.TrackingNumber,
                LabelUrl: transaction.LabelUrl,
                ProviderTrackerId: transaction.ObjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error purchasing label from Shippo for rate {RateId}", providerRateId);
            return ShippingErrors.ShippoError(ex.Message);
        }
    }

    private static ShippingRateQuote? MapRateToQuote(Rate rate)
    {
        if (string.IsNullOrWhiteSpace(rate.ObjectId) || string.IsNullOrWhiteSpace(rate.Amount))
        {
            return null;
        }

        if (!decimal.TryParse(rate.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var amountDecimal))
        {
            return null;
        }

        var carrier = CarrierCode.Create(rate.Provider ?? "UNKNOWN");
        var service = rate.Servicelevel?.Name ?? rate.Provider ?? "Standard";
        var price = Price.Create(amountDecimal);

        return ShippingRateQuote.Create(
            providerRateId: rate.ObjectId,
            carrier: carrier,
            service: service,
            price: price,
            currency: CurrencyCode.Create(rate.Currency),
            estDeliveryDays: (int?)rate.EstimatedDays);
    }
}


