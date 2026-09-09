using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Api.Domain;

namespace Notification.Api.Data.Configurations;

public class NotificationRecordConfiguration : IEntityTypeConfiguration<NotificationRecord>
{
    public void Configure(EntityTypeBuilder<NotificationRecord> builder)
    {
        builder.ToTable("notification_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReferenceId)
            .IsRequired();

        builder.Property(x => x.NotificationType)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Channel)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Recipient)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.Body)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(1024);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.ReferenceId, x.NotificationType, x.Channel })
            .IsUnique();

        builder.HasIndex(x => x.ReferenceId);
    }
}
