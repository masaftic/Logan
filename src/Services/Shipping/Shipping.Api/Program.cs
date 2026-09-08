using System.Text.Json.Serialization;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using JasperFx;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Shippo;
using Shipping.Api.Data;
using Shipping.Api.Features.CreateShipment;
using Shipping.Api.Features.EstimateShippingRates;
using Shipping.Api.Features.GetShipmentById;
using Shipping.Api.Features.GetShipmentByOrderId;
using Shipping.Api.Features.ShippoWebhook;
using Shipping.Api.Services;
using Shipping.Api.Services.Clients;
using Shipping.Api.Services.Packaging;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

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
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<ShippoOptions>(builder.Configuration.GetSection(ShippoOptions.SectionName));
builder.Services.Configure<WarehouseOptions>(builder.Configuration.GetSection(WarehouseOptions.SectionName));

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<ShippoOptions>>().Value;
    return new ShippoSDK(apiKeyHeader: options.ApiKey);
});

builder.Services.AddScoped<IShippingGateway, ShippoShippingGateway>();

var catalogUrl = builder.Configuration["Services:Catalog"] ?? "http://localhost:5005";
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
{
    client.BaseAddress = new Uri(catalogUrl);
});

builder.Services.AddScoped<IPackagingStrategy, CatalogPackagingStrategy>();

builder.Services.AddPostgresDbContext<ShippingDbContext>(builder.Configuration, schemaName: ShippingDbContext.SchemaName);

builder.Host.AddMessaging(
    builder.Configuration,
    applicationAssembly: typeof(Program).Assembly,
    schemaName: ShippingDbContext.SchemaName,
    configure: opts =>
    {
        opts.CodeGeneration.AlwaysUseServiceLocationFor<ICatalogClient>();
    });

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
    Service = "Shipping.Api",
    Schema = ShippingDbContext.SchemaName,
    Status = "Healthy",
    Timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapEstimateShippingRatesEndpoint();
app.MapCreateShipmentEndpoint();
app.MapGetShipmentByIdEndpoint();
app.MapGetShipmentByOrderIdEndpoint();
app.MapShippoWebhookEndpoint();

await app.ApplyMigrationsAsync<ShippingDbContext>();

return await app.RunJasperFxCommands(args);
