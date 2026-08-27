using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Thinktecture;

namespace BuildingBlocks.Persistence.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPostgresDbContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string? schemaName = null,
        string connectionName = "Database")
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' was not found in configuration.");

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(TContext).Assembly.FullName);
                if (!string.IsNullOrWhiteSpace(schemaName))
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schemaName);
                }
            })
            .UseSnakeCaseNamingConvention()
            .UseThinktectureValueConverters();
        });

        return services;
    }

    public static async Task ApplyMigrationsAsync<TContext>(this IHost app) where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<TContext>>();

        try
        {
            var dbContext = services.GetRequiredService<TContext>();
            logger.LogInformation("Applying EF Core migrations for {DbContextName}...", typeof(TContext).Name);

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("EF Core migrations applied successfully for {DbContextName}.", typeof(TContext).Name);
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync();
                logger.LogInformation("Database ensured created for {DbContextName}.", typeof(TContext).Name);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying EF Core migrations for {DbContextName}.", typeof(TContext).Name);
            throw;
        }
    }
}
