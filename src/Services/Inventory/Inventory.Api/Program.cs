using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using Inventory.Api.Data;
using Inventory.Api.Data.Seeders;
using Inventory.Api.Features.GetStock;
using Inventory.Api.Features.ReserveStock;
using JasperFx;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddPostgresDbContext<InventoryDbContext>(builder.Configuration, schemaName: InventoryDbContext.SchemaName);

builder.Host.AddMessaging(
    builder.Configuration, 
    applicationAssembly: typeof(Program).Assembly,
    schemaName: InventoryDbContext.SchemaName);

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

app.MapGetStockEndpoint();
app.MapReserveStockEndpoint();

await app.ApplyMigrationsAsync<InventoryDbContext>();
await app.SeedInventoryAsync();

return await app.RunJasperFxCommands(args);
