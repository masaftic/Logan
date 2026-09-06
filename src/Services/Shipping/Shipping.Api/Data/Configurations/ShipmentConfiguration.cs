using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Api.Domain;
using Thinktecture;

namespace Shipping.Api.Data.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.ComplexProperty(x => x.OriginAddress, b =>
        {
            b.ToJson();
            b.AddThinktectureValueConverters();
        });

        builder.ComplexProperty(x => x.DestinationAddress, b =>
        {
            b.ToJson();
            b.AddThinktectureValueConverters();
        });

        builder.ComplexProperty(x => x.Dimensions, b =>
        {
            b.ComplexProperty(x => x.Length);
            b.ComplexProperty(x => x.Width);
            b.ComplexProperty(x => x.Height);
            b.ToJson();
            b.AddThinktectureValueConverters();
        });

        builder.ComplexProperty(x => x.Weight, b =>
        {
            b.ToJson();
            b.AddThinktectureValueConverters();
        });

        builder.ComplexProperty(x => x.SelectedRate, b =>
        {
            b.ToJson();
            b.AddThinktectureValueConverters();
        });

        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(128);

        builder.Property(x => x.LabelUrl)
            .HasMaxLength(1024);

        builder.Property(x => x.ProviderShipmentId)
            .HasMaxLength(128);

        builder.Property(x => x.ProviderTrackerId)
            .HasMaxLength(128);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.DispatchedAtUtc);
        builder.Property(x => x.DeliveredAtUtc);
        builder.Property(x => x.CancelledAtUtc);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(512);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.TrackingMilestones)
            .WithOne()
            .HasForeignKey(x => x.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.TrackingMilestones)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ProviderShipmentId);
        builder.HasIndex(x => x.TrackingNumber);
    }
}
