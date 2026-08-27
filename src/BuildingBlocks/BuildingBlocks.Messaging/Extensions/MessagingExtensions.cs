using System.Reflection;
using JasperFx.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.FluentValidation;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace BuildingBlocks.Messaging.Extensions;

public static class MessagingExtensions
{
    public static IHostBuilder AddMessaging(
        this IHostBuilder hostBuilder,
        IConfiguration configuration,
        Assembly applicationAssembly,
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

            opts.ApplicationAssembly = applicationAssembly;

            // RabbitMQ Transport with conventional routing for Commands and Events (excluding Queries)
            opts.UseRabbitMq(rabbitUri)
                .AutoProvision()
                .UseConventionalRouting(conventions =>
                {
                    conventions.IncludeTypes(type =>
                        (type.Namespace?.Contains("Queries") != true) &&
                        !type.Name.EndsWith("Query"));
                });

            // Persist message envelopes in PostgreSQL
            opts.PersistMessagesWithPostgresql(dbConnectionString, schemaName);

            // EF Core Transaction Integration
            opts.UseEntityFrameworkCoreTransactions();
            opts.Policies.AutoApplyTransactions();

            opts.UseFluentValidation();

            // Global Resilience Policy (Exponential Backoff for Transient Failures)
            opts.OnException<Exception>()
                .RetryWithCooldown(
                    200.Milliseconds(),
                    500.Milliseconds(),
                    2.Seconds()
                );

            // Service-specific configurations (Sagas, Discovery, etc.)
            configure?.Invoke(opts);
        });
    }
}
