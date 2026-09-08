using System.Text.Json.Serialization;
using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Messaging.Extensions;
using BuildingBlocks.Persistence.Extensions;
using JasperFx;
using Payment.Api.Data;
using Payment.Api.Features.GetPayment;
using Payment.Api.Features.InitializePayment;
using Payment.Api.Features.RefundPayment;
using Payment.Api.Features.StripeWebhook;
using Payment.Api.Features.TestConfirm;
using Payment.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection(StripeOptions.SectionName));
builder.Services.AddSingleton<IStripePaymentGateway, StripePaymentGateway>();

builder.Services.AddPostgresDbContext<PaymentDbContext>(builder.Configuration, schemaName: PaymentDbContext.SchemaName);

builder.Host.AddMessaging(
    builder.Configuration,
    applicationAssembly: typeof(Program).Assembly,
    schemaName: PaymentDbContext.SchemaName);

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
    Service = "Payment.Api",
    Schema = PaymentDbContext.SchemaName,
    Status = "Healthy",
    Timestamp = DateTime.UtcNow
})).WithTags("Health");

app.MapInitializePaymentEndpoint();
app.MapGetPaymentByOrderIdEndpoint();
app.MapRefundPaymentEndpoint();
app.MapStripeWebhookEndpoint();
app.MapTestConfirmEndpoint();

await app.ApplyMigrationsAsync<PaymentDbContext>();

return await app.RunJasperFxCommands(args);
