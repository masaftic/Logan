using Shipping.Contracts.DTOs;

namespace Shipping.Contracts.Commands;

public record CommandCreateShipment(
    Guid OrderId,
    string ProviderRateId,
    ShippingAddressDto DestinationAddress,
    IReadOnlyList<ShipmentItemDto> Items);

