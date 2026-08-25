using BuildingBlocks.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => Results.Ok(new
{
    Gateway = "API Gateway (YARP)",
    Status = "Healthy",
    Routes = new[] { "/api/orders/*", "/api/inventory/*" },
    Timestamp = DateTime.UtcNow
}));

app.MapReverseProxy();

app.Run();
