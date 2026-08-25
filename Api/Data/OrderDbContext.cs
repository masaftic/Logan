using Api.Domain.Orders;
using Api.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("orders");

            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).ValueGeneratedNever();

            builder.Property(o => o.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(o => o.CreatedAtUtc)
                .IsRequired();

            builder.Property(o => o.UpdatedAtUtc);

            builder.HasMany(o => o.Payments)
                .WithOne()
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(o => o.Payments)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Payment>(builder =>
        {
            builder.ToTable("payments");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedNever();

            builder.Property(p => p.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();

            builder.Property(p => p.IdempotencyKey)
                .HasMaxLength(128)
                .IsRequired();

            builder.HasIndex(p => p.IdempotencyKey)
                .IsUnique();

            builder.Property(p => p.GatewayTransactionId)
                .HasMaxLength(128);

            builder.Property(p => p.FailureReason)
                .HasMaxLength(512);

            builder.Property(p => p.CreatedAtUtc)
                .IsRequired();

            builder.Property(p => p.CompletedAtUtc);
        });
    }
}
