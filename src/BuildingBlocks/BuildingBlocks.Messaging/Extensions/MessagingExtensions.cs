using JasperFx.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace BuildingBlocks.Messaging.Extensions;

public static class MessagingExtensions
{
    public static IHostBuilder AddMessaging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        string? schemaName = null,
        Action<WolverineOptions>? configure = null)
    {
        return hostBuilder.UseWolverine(opts =>
        {
            var rabbitUriString = configuration.GetConnectionString("RabbitMQ")
                ?? "amqp://guest:guest@localhost:5672";
            var rabbitUri = new Uri(rabbitUriString);

            var dbConnectionString = configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("Database connection string is required for Wolverine message persistence.");

            // 1. RabbitMQ Transport with auto-provisioning and conventions
            opts.UseRabbitMq(rabbitUri)
                .AutoProvision()
                .UseConventionalRouting();

            // 2. Persist message envelopes in PostgreSQL (using service schema if provided)
            if (!string.IsNullOrWhiteSpace(schemaName))
            {
                opts.PersistMessagesWithPostgresql(dbConnectionString, schemaName);
            }
            else
            {
                opts.PersistMessagesWithPostgresql(dbConnectionString);
            }

            // 3. EF Core Transaction Integration
            opts.UseEntityFrameworkCoreTransactions();
            opts.Policies.AutoApplyTransactions();

            // 4. Global Resilience Policy (Exponential Backoff for Transient Failures)
            opts.OnException<Exception>()
                .RetryWithCooldown(
                    200.Milliseconds(),
                    500.Milliseconds(),
                    2.Seconds()
                );

            // 5. Service-specific configurations (Sagas, Discovery, etc.)
            configure?.Invoke(opts);
        });
    }
}
