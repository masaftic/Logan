using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shipping.Api.Data;
using Shipping.Api.Data.Extensions;
using Shipping.Api.Domain;
using Shipping.Api.Domain.ValueObjects;
using Shipping.Api.Services;
using Shipping.Contracts.Commands;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Enums;

namespace Shipping.Api.Features.CreateShipment;

public class CreateShipmentHandler
{
    private readonly ShippingDbContext _dbContext;
    private readonly IShippingGateway _shippingGateway;
    private readonly WarehouseOptions _warehouseOptions;

    public CreateShipmentHandler(
        ShippingDbContext dbContext,
        IShippingGateway shippingGateway,
        IOptions<WarehouseOptions> warehouseOptions)
    {
        _dbContext = dbContext;
        _shippingGateway = shippingGateway;
        _warehouseOptions = warehouseOptions.Value;
    }

    public async Task<Result<ShipmentDto>> Handle(
        CommandCreateShipment command,
        CancellationToken cancellationToken)
    {
        var existingShipment = await _dbContext.Shipments
            .Where(s => s.OrderId == command.OrderId)
            .WhereActiveOrPurchased()
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingShipment is not null)
        {
            return MapToDto(existingShipment);
        }

        var destCountry = CountryCode.TryGet(command.DestinationAddress.Country, out var country)
            ? country
            : CountryCode.US;

        var destinationAddress = Address.Create(
            Street.Create(command.DestinationAddress.Street1),
            command.DestinationAddress.Street2 is not null ? Street.Create(command.DestinationAddress.Street2) : null,
            City.Create(command.DestinationAddress.City),
            StateOrProvince.Create(command.DestinationAddress.State),
            PostalCode.Create(command.DestinationAddress.PostalCode),
            destCountry);

        var originAddress = Address.Create(
            Street.Create(_warehouseOptions.Street1),
            null,
            City.Create(_warehouseOptions.City),
            StateOrProvince.Create(_warehouseOptions.State),
            PostalCode.Create(_warehouseOptions.Zip),
            CountryCode.US);


        var lengthUnit = LengthUnit.TryGet(command.Dimensions.Unit, out var lUnit)
            ? lUnit
            : LengthUnit.Centimeter;

        var dimensions = PackageDimensions.Create(
            command.Dimensions.Length,
            command.Dimensions.Width,
            command.Dimensions.Height,
            lengthUnit);

        var weightUnit = WeightUnit.TryGet(command.Weight.Unit, out var wUnit)
            ? wUnit
            : WeightUnit.Gram;

        var weight = Weight.Create(command.Weight.Value, weightUnit);

        var shipmentId = ShipmentId.New();
        var domainItems = command.Items.Select(item =>
            ShipmentItem.Create(
                ShipmentItemId.New(),
                shipmentId,
                Sku.Create(item.Sku),
                PositiveQuantity.Create(item.Quantity))
        ).ToList();

        var rateResult = await _shippingGateway.GetRateByIdAsync(command.ProviderRateId, cancellationToken);
        if (rateResult.IsError)
        {
            return rateResult.Errors;
        }

        var selectedRateQuote = rateResult.Value;

        var draftResult = Shipment.CreateDraft(
            shipmentId,
            command.OrderId,
            originAddress,
            destinationAddress,
            dimensions,
            weight,
            domainItems);

        if (draftResult.IsError)
        {
            return draftResult.Errors;
        }

        var shipment = draftResult.Value;

        var labelResult = await _shippingGateway.PurchaseLabelAsync(
            command.ProviderRateId,
            cancellationToken);

        if (labelResult.IsError)
        {
            return labelResult.Errors;
        }

        var purchased = labelResult.Value;
        var purchaseResult = shipment.PurchaseLabel(
            selectedRateQuote,
            TrackingNumber.Create(purchased.TrackingNumber),
            purchased.LabelUrl,
            providerShipmentId: purchased.ProviderTransactionId,
            providerTrackerId: purchased.ProviderTrackerId ?? purchased.ProviderTransactionId);

        if (purchaseResult.IsError)
        {
            return purchaseResult.Errors;
        }

        _dbContext.Shipments.Add(shipment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(shipment);
    }

    private static ShipmentDto MapToDto(Shipment shipment) => new(
        Id: shipment.Id,
        OrderId: shipment.OrderId,
        Status: (ShippingStatusDto)shipment.Status,
        Carrier: shipment.SelectedRate?.Carrier,
        Service: shipment.SelectedRate?.Service,
        RateAmount: shipment.SelectedRate?.Price,
        Currency: shipment.SelectedRate?.Currency,
        TrackingNumber: shipment.TrackingNumber,
        LabelUrl: shipment.LabelUrl,
        CreatedAtUtc: shipment.CreatedAtUtc,
        DispatchedAtUtc: shipment.DispatchedAtUtc,
        FailureReason: shipment.FailureReason);
}
