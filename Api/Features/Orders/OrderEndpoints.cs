using Api.Features.Orders.CancelOrder;
using Api.Features.Orders.CreateOrder;
using Api.Features.Orders.GetOrderById;
using Api.Features.Orders.GetOrders;

namespace Api.Features.Orders;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapCreateOrderEndpoint();
        group.MapGetOrderByIdEndpoint();
        group.MapGetOrdersEndpoint();
        group.MapCancelOrderEndpoint();

        return app;
    }
}
