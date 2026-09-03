using System.Text.Json.Serialization;
using ApiGateway.Features.Checkout;
using BuildingBlocks.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var orderingUrl = builder.Configuration["Services:Ordering"] ?? "http://localhost:5001";
var paymentUrl = builder.Configuration["Services:Payment"] ?? "http://localhost:5003";

builder.Services.AddHttpClient<IOrderingClient, OrderingClient>(client =>
{
    client.BaseAddress = new Uri(orderingUrl);
});

builder.Services.AddHttpClient<IPaymentClient, PaymentClient>(client =>
{
    client.BaseAddress = new Uri(paymentUrl);
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    Gateway = "API Gateway (YARP)",
    Status = "Healthy",
    Routes = new[] { "/api/checkout", "/api/orders/*", "/api/inventory/*", "/api/payments/*" },
    Timestamp = DateTime.UtcNow
}));

app.MapCheckoutEndpoint();

app.MapReverseProxy();

app.Run();
