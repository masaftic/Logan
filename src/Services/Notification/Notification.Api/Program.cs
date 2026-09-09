using System.Text.Json.Serialization;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using JasperFx;
using Notification.Api.Data;
using Notification.Api.Features.GetNotifications;
using Notification.Api.Services.Email;
using Notification.Api.Services.Sms;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<SmtpOptions>(options =>
{
    builder.Configuration.GetSection(SmtpOptions.SectionName).Bind(options);

    var aspireEndpoint = builder.Configuration["services:mailpit:smtp:0"]
        ?? builder.Configuration["services:mailpit:default:0"];
    if (!string.IsNullOrWhiteSpace(aspireEndpoint) && Uri.TryCreate(aspireEndpoint, UriKind.Absolute, out var uri))
    {
        options.Host = uri.Host;
        options.Port = uri.Port;
    }
});
builder.Services.AddScoped<IEmailSender, MailpitSmtpEmailSender>();
builder.Services.AddScoped<ISmsSender, LoggingSmsSender>();

builder.Services.AddPostgresDbContext<NotificationDbContext>(builder.Configuration, schemaName: NotificationDbContext.SchemaName);

builder.Host.AddMessaging(
    builder.Configuration,
    applicationAssembly: typeof(Program).Assembly,
    schemaName: NotificationDbContext.SchemaName);

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
    Service = "Notification.Api",
    Schema = NotificationDbContext.SchemaName,
    Status = "Healthy",
    Timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapGetNotificationsEndpoint();

await app.ApplyMigrationsAsync<NotificationDbContext>();

return await app.RunJasperFxCommands(args);
