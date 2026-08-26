using Microsoft.EntityFrameworkCore;

namespace Ordering.Api.Data;

public class OrderDbContext : DbContext
{
    public const string SchemaName = "ordering";

    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}
