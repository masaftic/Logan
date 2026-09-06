using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipping.Api.Domain;

namespace Shipping.Api.Data.Configurations;

public class TrackingMilestoneConfiguration : IEntityTypeConfiguration<TrackingMilestone>
{
    public void Configure(EntityTypeBuilder<TrackingMilestone> builder)
    {
        builder.ToTable("tracking_milestones");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ShipmentId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Message)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Location)
            .HasMaxLength(256);

        builder.Property(x => x.OccurredAtUtc)
            .IsRequired();

        builder.HasIndex(x => x.ShipmentId);
    }
}
