using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using Inventory.Api.Data;
using JasperFx;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddPostgresDbContext<InventoryDbContext>(builder.Configuration, schemaName: InventoryDbContext.SchemaName);

builder.Host.AddMessaging(builder.Configuration, schemaName: InventoryDbContext.SchemaName);

var app = builder.Build();

app.MapDefaultEndpoints();

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
}));

app.MapGet("/api/inventory/hello", () => Results.Ok(new
{
    Message = "Hello from Inventory Service!",
    Schema = InventoryDbContext.SchemaName,
    Timestamp = DateTime.UtcNow
}));

await app.ApplyMigrationsAsync<InventoryDbContext>();

return await app.RunJasperFxCommands(args);
