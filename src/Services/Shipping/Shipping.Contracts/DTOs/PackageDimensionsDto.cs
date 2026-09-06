namespace Shipping.Contracts.DTOs;

public record PackageDimensionsDto(
    decimal Length,
    decimal Width,
    decimal Height,
    string Unit);
