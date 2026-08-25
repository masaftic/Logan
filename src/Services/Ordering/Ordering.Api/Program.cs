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
    Service = "Ordering.Api",
    Status = "Healthy",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow
}));

app.MapGet("/api/orders/hello", () => Results.Ok(new
{
    Message = "Hello from Ordering Service!",
    OrderId = Guid.NewGuid(),
    CreatedAt = DateTime.UtcNow
}));

app.Run();
