using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using Catalog.Api.Data;
using Catalog.Api.Data.Seeders;
using Catalog.Api.Features.GetCategories;
using Catalog.Api.Features.GetCategoryById;
using Catalog.Api.Features.GetProducts;
using Catalog.Api.Features.GetProductById;
using Catalog.Api.Features.GetProductBySku;
using Catalog.Api.Features.GetProductsBySkus;
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

builder.Services.AddPostgresDbContext<CatalogDbContext>(builder.Configuration, schemaName: CatalogDbContext.SchemaName);

builder.Host.AddMessaging(
    builder.Configuration,
    applicationAssembly: typeof(Program).Assembly,
    schemaName: CatalogDbContext.SchemaName);

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
    Service = "Catalog.Api",
    Schema = CatalogDbContext.SchemaName,
    Status = "Healthy",
    Timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapGetCategoriesEndpoint();
app.MapGetCategoryByIdEndpoint();
app.MapGetProductsEndpoint();
app.MapGetProductByIdEndpoint();
app.MapGetProductBySkuEndpoint();
app.MapGetProductsBySkusEndpoint();

await app.ApplyMigrationsAsync<CatalogDbContext>();
await app.SeedCatalogAsync();

return await app.RunJasperFxCommands(args);
