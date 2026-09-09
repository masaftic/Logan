using Microsoft.EntityFrameworkCore;
using Notification.Api.Domain;

namespace Notification.Api.Data;

public class NotificationDbContext : DbContext
{
    public const string SchemaName = "notifications";

    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();

    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
