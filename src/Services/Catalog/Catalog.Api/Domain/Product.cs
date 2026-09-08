using BuildingBlocks.Common.ValueObjects;

namespace Catalog.Api.Domain;

public class Product
{
    public Guid Id { get; private set; }
    public Sku Sku { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Price Price { get; private set; }
    public CurrencyCode Currency { get; private set; } = null!;
    public Weight Weight { get; private set; } = null!;
    public PackageDimensions Dimensions { get; private set; } = null!;
    public Guid CategoryId { get; private set; }
    public Category Category { get; private set; } = null!;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    private Product() { }

    public static Product Create(
        Guid id,
        Sku sku,
        string name,
        string? description,
        Price price,
        CurrencyCode currency,
        Weight weight,
        PackageDimensions dimensions,
        Guid categoryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(currency);

        return new Product
        {
            Id = id,
            Sku = sku,
            Name = name.Trim(),
            Description = description,
            Price = price,
            Currency = currency,
            Weight = weight,
            Dimensions = dimensions,
            CategoryId = categoryId,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}
