using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shipping.Api.Data;
using Shipping.Api.Data.Extensions;
using Shipping.Api.Domain;
using Shipping.Api.Domain.ValueObjects;
using Shipping.Api.Services;
using Shipping.Api.Services.Packaging;
using Shipping.Contracts.Commands;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Enums;
using Shipping.Contracts.Events;
using Wolverine;

namespace Shipping.Api.Features.CreateShipment;

public class CreateShipmentHandler
{
    private readonly ShippingDbContext _dbContext;
    private readonly IShippingGateway _shippingGateway;
    private readonly WarehouseOptions _warehouseOptions;
    private readonly IMessageBus _bus;
    private readonly IPackagingStrategy _packagingStrategy;

    public CreateShipmentHandler(
        ShippingDbContext dbContext,
        IShippingGateway shippingGateway,
        IOptions<WarehouseOptions> warehouseOptions,
        IMessageBus bus,
        IPackagingStrategy packagingStrategy)
    {
        _dbContext = dbContext;
        _shippingGateway = shippingGateway;
        _warehouseOptions = warehouseOptions.Value;
        _bus = bus;
        _packagingStrategy = packagingStrategy;
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

        var packageResult = await _packagingStrategy.CalculatePackageAsync(command.Items, cancellationToken);
        if (packageResult.IsError)
        {
            await _bus.PublishAsync(new ShipmentCreationFailedEvent(command.OrderId, packageResult.FirstError.Description, DateTime.UtcNow));
            return packageResult.Errors;
        }

        var dimensions = packageResult.Value.Dimensions;
        var weight = packageResult.Value.Weight;

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
            await _bus.PublishAsync(new ShipmentCreationFailedEvent(command.OrderId, rateResult.FirstError.Description, DateTime.UtcNow));
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
            await _bus.PublishAsync(new ShipmentCreationFailedEvent(command.OrderId, draftResult.FirstError.Description, DateTime.UtcNow));
            return draftResult.Errors;
        }

        var shipment = draftResult.Value;

        var labelResult = await _shippingGateway.PurchaseLabelAsync(
            command.ProviderRateId,
            cancellationToken);

        if (labelResult.IsError)
        {
            await _bus.PublishAsync(new ShipmentCreationFailedEvent(command.OrderId, labelResult.FirstError.Description, DateTime.UtcNow));
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
            await _bus.PublishAsync(new ShipmentCreationFailedEvent(command.OrderId, purchaseResult.FirstError.Description, DateTime.UtcNow));
            return purchaseResult.Errors;
        }

        _dbContext.Shipments.Add(shipment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _bus.PublishAsync(new ShipmentLabelPurchasedEvent(
            ShipmentId: shipment.Id,
            OrderId: shipment.OrderId,
            TrackingNumber: shipment.TrackingNumber!,
            Carrier: shipment.SelectedRate?.Carrier ?? "UNKNOWN",
            LabelUrl: shipment.LabelUrl!,
            DispatchedAtUtc: shipment.DispatchedAtUtc ?? DateTime.UtcNow));

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
