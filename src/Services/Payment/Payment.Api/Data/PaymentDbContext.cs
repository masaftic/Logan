using Microsoft.EntityFrameworkCore;
using Payment.Api.Domain;

namespace Payment.Api.Data;

public class PaymentDbContext : DbContext
{
    public const string SchemaName = "payments";

    public DbSet<PaymentRecord> Payments => Set<PaymentRecord>();
    public DbSet<PaymentRefund> PaymentRefunds => Set<PaymentRefund>();

    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(SchemaName);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PaymentDbContext).Assembly);
    }
}
