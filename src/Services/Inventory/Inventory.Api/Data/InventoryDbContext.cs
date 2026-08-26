using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data;

public class InventoryDbContext : DbContext
{
    public const string SchemaName = "inventory";

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
    }
}
