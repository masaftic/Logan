using Microsoft.EntityFrameworkCore;
using Ordering.Api.Domain;
using Ordering.Api.Domain.ReadModels;

namespace Ordering.Api.Data;

public class OrderDbContext : DbContext
{
    public const string SchemaName = "ordering";

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderSummary> OrderSummaries => Set<OrderSummary>();

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }
}
