using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var ordering = builder.AddProject<Projects.Ordering_Api>("ordering-api")
    .WithHttpEndpoint(port: 5001, name: "http");

var inventory = builder.AddProject<Projects.Inventory_Api>("inventory-api")
    .WithHttpEndpoint(port: 5002, name: "http");

var payment = builder.AddProject<Projects.Payment_Api>("payment-api")
    .WithHttpEndpoint(port: 5003, name: "http");

var shipping = builder.AddProject<Projects.Shipping_Api>("shipping-api")
    .WithHttpEndpoint(port: 5004, name: "http");

var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WaitFor(ordering)
    .WaitFor(inventory)
    .WaitFor(payment)
    .WaitFor(shipping);

builder.Build().Run();
