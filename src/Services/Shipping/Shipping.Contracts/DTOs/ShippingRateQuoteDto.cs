namespace Shipping.Contracts.DTOs;

public record ShippingRateQuoteDto(
    string ProviderRateId,
    string Carrier,
    string Service,
    decimal Amount,
    string Currency,
    int? EstDeliveryDays,
    string? DurationTerms);
