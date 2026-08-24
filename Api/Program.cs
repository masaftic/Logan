using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Api.Data;
using Api.Endpoints;
using Contracts;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (context.JsonTypeInfo.Type.IsEnum)
        {
            schema.Type = JsonSchemaType.String;
            schema.Enum = [
                ..Enum.GetNames(context.JsonTypeInfo.Type).Select(name => JsonValue.Create(name))
            ];
        }
        return Task.CompletedTask;
    });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddValidation();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("Database")!;

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Host.UseWolverine(opts =>
{
    var rabbitUri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")!);

    // 1. Configure RabbitMQ transport with auto-provisioning & conventions
    opts.UseRabbitMq(rabbitUri)
        .AutoProvision()
        .UseConventionalRouting();

    // 2. Persist message envelopes (Outbox/Inbox/Timeouts/DLQ) in PostgreSQL
    opts.PersistMessagesWithPostgresql(connectionString);

    // 3. Integrate Wolverine with EF Core DbContext transactions
    opts.UseEntityFrameworkCoreTransactions();

    // 4. Automatically manage transactions and SaveChangesAsync for handlers using DbContext
    opts.Policies.AutoApplyTransactions();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapOrderEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    await dbContext.Database.MigrateAsync();

    await dbContext.Orders.ExecuteDeleteAsync();
}

return await app.RunJasperFxCommands(args);
