using Catalog.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Thinktecture;

namespace Catalog.Api.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.ComplexProperty(p => p.Dimensions, b =>
        {
            b.ComplexProperty(d => d.Length);
            b.ComplexProperty(d => d.Width);
            b.ComplexProperty(d => d.Height);
            b.ToJson();
            b.AddThinktectureValueConverters();
        });

        builder.ComplexProperty(p => p.Weight, b =>
        {
            b.ToJson();
            b.AddThinktectureValueConverters();
        });

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
