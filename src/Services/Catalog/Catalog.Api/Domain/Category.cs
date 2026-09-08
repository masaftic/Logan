namespace Catalog.Api.Domain;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Category() { }

    public static Category Create(Guid id, string name, string slug, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Category
        {
            Id = id,
            Name = name,
            Slug = slug.ToLowerInvariant(),
            Description = description,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
