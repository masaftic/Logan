using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var inventory = builder.AddProject<Projects.Inventory_Api>("inventory-api")
    .WithHttpEndpoint(port: 5002, name: "http");

var catalog = builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithHttpEndpoint(port: 5005, name: "http")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development");

var ordering = builder.AddProject<Projects.Ordering_Api>("ordering-api")
    .WithHttpEndpoint(port: 5001, name: "http")
    .WithReference(inventory)
    .WaitFor(inventory)
    .WithReference(catalog)
    .WaitFor(catalog);

var payment = builder.AddProject<Projects.Payment_Api>("payment-api")
    .WithHttpEndpoint(port: 5003, name: "http");

var shipping = builder.AddProject<Projects.Shipping_Api>("shipping-api")
    .WithHttpEndpoint(port: 5004, name: "http")
    .WithReference(catalog)
    .WaitFor(catalog);

var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WaitFor(ordering)
    .WaitFor(inventory)
    .WaitFor(payment)
    .WaitFor(shipping)
    .WaitFor(catalog);

builder.Build().Run();
