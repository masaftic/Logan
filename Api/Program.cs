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

    opts.UseRabbitMq(rabbitUri)
        .AutoProvision()
        .UseConventionalRouting();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapOrderEndpoints();

// Automatically apply pending EF Core migrations on application startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    Console.WriteLine(dbContext.Database.CanConnect());

    await dbContext.Database.MigrateAsync();
}

return await app.RunJasperFxCommands(args);
