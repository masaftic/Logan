using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using FluentValidation;
using JasperFx;
using Ordering.Api.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
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
}));

app.MapGet("/api/orders/hello", () => Results.Ok(new
{
    Message = "Hello from Ordering Service!",
    Schema = OrderDbContext.SchemaName,
    Timestamp = DateTime.UtcNow
}));

await app.ApplyMigrationsAsync<OrderDbContext>();

return await app.RunJasperFxCommands(args);
