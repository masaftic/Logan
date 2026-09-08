namespace Catalog.Contracts.DTOs;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description
);
