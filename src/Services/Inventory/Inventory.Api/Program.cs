using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using Inventory.Api.Data;
using Inventory.Api.Data.Seeders;
using Inventory.Api.Features.ConfirmStockDeduction;
using Inventory.Api.Features.GetStock;
using Inventory.Api.Features.GetStockHistory;
using Inventory.Api.Features.GetStockList;
using Inventory.Api.Features.ReleaseStock;
using Inventory.Api.Features.ReserveStock;
using Inventory.Api.Features.RestockItem;
using JasperFx;
using Microsoft.OpenApi;
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

builder.Services.AddPostgresDbContext<InventoryDbContext>(builder.Configuration, schemaName: InventoryDbContext.SchemaName);

builder.Host.AddMessaging(
    builder.Configuration,
    applicationAssembly: typeof(Program).Assembly,
    schemaName: InventoryDbContext.SchemaName);

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseValidationExceptionHandler();
app.UseRequestShapeLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/", () => Results.Ok(new
{
    Service = "Inventory.Api",
    Schema = InventoryDbContext.SchemaName,
    Status = "Healthy",
    Timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapGetStockEndpoint();
app.MapGetStockListEndpoint();
app.MapGetStockHistoryEndpoint();
app.MapReserveStockEndpoint();
app.MapReleaseStockEndpoint();
app.MapConfirmStockDeductionEndpoint();
app.MapRestockItemEndpoint();

await app.ApplyMigrationsAsync<InventoryDbContext>();
await app.SeedInventoryAsync();

return await app.RunJasperFxCommands(args);
