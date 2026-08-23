using Api.Data;
using Api.DTOs;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapPost("/", async (CreateOrderRequest request, OrderDbContext dbContext, CancellationToken ct) =>
        {
            var order = new Order
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Status = OrderStatus.Pending,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync(ct);

            var response = OrderResponse.FromEntity(order);
            return Results.Created($"/api/orders/{order.Id}", response);
        })
        .WithName("CreateOrder")
        .WithSummary("Place a new order")
        .Produces<OrderResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        group.MapGet("/{id:guid}", async (Guid id, OrderDbContext dbContext, CancellationToken ct) =>
        {
            var order = await dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            return order is not null
                ? Results.Ok(OrderResponse.FromEntity(order))
                : Results.NotFound(new { Message = $"Order with ID '{id}' was not found." });
        })
        .WithName("GetOrderById")
        .WithSummary("Fetch order details by ID")
        .Produces<OrderResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", async (OrderDbContext dbContext, CancellationToken ct) =>
        {
            var orders = await dbContext.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAtUtc)
                .Select(o => OrderResponse.FromEntity(o))
                .ToListAsync(ct);

            return Results.Ok(orders);
        })
        .WithName("GetOrders")
        .WithSummary("List all orders")
        .Produces<List<OrderResponse>>(StatusCodes.Status200OK);

        return app;
    }
}
