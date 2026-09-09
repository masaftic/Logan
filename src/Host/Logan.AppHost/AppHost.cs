using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var inventory = builder.AddProject<Projects.Inventory_Api>("inventory-api")
    .WithHttpEndpoint(port: 5002, name: "http");

var catalog = builder.AddProject<Projects.Catalog_Api>("catalog-api")
    .WithHttpEndpoint(port: 5005, name: "http");

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

var mailpit = builder.AddContainer("mailpit", "axllent/mailpit")
    .WithHttpEndpoint(port: 8025, targetPort: 8025, name: "web")
    .WithEndpoint(port: 1025, targetPort: 1025, name: "smtp");

var notification = builder.AddProject<Projects.Notification_Api>("notification-api")
    .WithHttpEndpoint(port: 5006, name: "http")
    .WithReference(mailpit.GetEndpoint("smtp"))
    .WaitFor(mailpit);

var apiGateway = builder.AddProject<Projects.ApiGateway>("api-gateway")
    .WithHttpEndpoint(port: 5000, name: "http")
    .WaitFor(ordering)
    .WaitFor(inventory)
    .WaitFor(payment)
    .WaitFor(shipping)
    .WaitFor(catalog)
    .WaitFor(notification);

builder.Build().Run();
