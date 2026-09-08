using System.Reflection;
using JasperFx.CodeGeneration.Model;
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
            var rabbitUri = new Uri(configuration.GetConnectionString("RabbitMQ")!);
            var dbConnectionString = configuration.GetConnectionString("Database")!;

            opts.ApplicationAssembly = applicationAssembly;
            opts.ServiceLocationPolicy = ServiceLocationPolicy.AllowedButWarn;

            // RabbitMQ Transport with conventional routing for Commands and Events (excluding Queries, DTOs, and internal results)
            opts.UseRabbitMq(rabbitUri)
                .AutoProvision()
                .UseConventionalRouting(conventions =>
                {
                    conventions.IncludeTypes(type =>
                        (type.Namespace?.EndsWith("Commands") == true || type.Name.EndsWith("Command") ||
                         type.Namespace?.EndsWith("Events") == true || type.Name.EndsWith("Event")) &&
                        type.Namespace?.Contains("Queries") != true &&
                        !type.Name.EndsWith("Query") &&
                        type.Assembly.GetName().Name != "BuildingBlocks.Common");
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
