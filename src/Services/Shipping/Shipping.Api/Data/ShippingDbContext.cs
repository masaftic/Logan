using Microsoft.EntityFrameworkCore;
using Shipping.Api.Domain;

namespace Shipping.Api.Data;

public class ShippingDbContext : DbContext
{
    public const string SchemaName = "shipping";

    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<ShipmentItem> ShipmentItems => Set<ShipmentItem>();
    public DbSet<TrackingMilestone> TrackingMilestones => Set<TrackingMilestone>();

    public ShippingDbContext(DbContextOptions<ShippingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShippingDbContext).Assembly);
    }
}
