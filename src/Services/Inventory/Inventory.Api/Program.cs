using BuildingBlocks.Common.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add standard OpenTelemetry, health checks, and service defaults
builder.AddServiceDefaults();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

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
    Status = "Healthy",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow
}));

app.MapGet("/api/inventory/hello", () => Results.Ok(new
{
    Message = "Hello from Inventory Service!",
    AvailableItems = 42,
    Timestamp = DateTime.UtcNow
}));

app.Run();
