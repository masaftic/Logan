using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using JasperFx;
using Microsoft.OpenApi;
using Ordering.Api.Data;
using Ordering.Api.Features.CancelOrder;
using Ordering.Api.Features.SubmitOrder;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer((schema, context, cancellationToken) =>
    {
        if (context.JsonTypeInfo.Type == typeof(Guid))
        {
            schema.Example = JsonValue.Create(Guid.NewGuid().ToString());
        }

        return Task.CompletedTask;
    });

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
builder.Services.AddProblemDetails();

builder.Services.AddPostgresDbContext<OrderDbContext>(builder.Configuration, schemaName: OrderDbContext.SchemaName);

builder.Host.AddMessaging(
    builder.Configuration, 
    applicationAssembly: typeof(Program).Assembly,
    schemaName: OrderDbContext.SchemaName);

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Ok(new
{
    Service = "Ordering.Api",
    Schema = OrderDbContext.SchemaName,
    Status = "Healthy",
    Timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapSubmitOrderEndpoint();
app.MapCancelOrderEndpoint();

await app.ApplyMigrationsAsync<OrderDbContext>();

return await app.RunJasperFxCommands(args);
