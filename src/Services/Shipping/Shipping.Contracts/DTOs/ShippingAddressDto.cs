namespace Shipping.Contracts.DTOs;

public record ShippingAddressDto(
    string Street1,
    string? Street2,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? Name = null,
    string? Phone = null);
