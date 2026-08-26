using Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Api.Data.Configurations;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");

        builder.HasKey(x => x.Sku);

        builder.Property(x => x.Sku)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.QuantityAvailable)
            .IsRequired();

        builder.Property(x => x.QuantityReserved)
            .IsRequired();

        builder.Property(x => x.QuantityOnHand)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}

