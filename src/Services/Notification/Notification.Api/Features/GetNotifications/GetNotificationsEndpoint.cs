using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Notification.Contracts.DTOs;
using Notification.Contracts.Queries;
using Wolverine;

namespace Notification.Api.Features.GetNotifications;

public static class GetNotificationsEndpoint
{
    public static void MapGetNotificationsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/notifications/orders/{orderId:guid}", async (
            Guid orderId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new QueryGetNotificationsByOrderId(orderId);
            var result = await bus.InvokeAsync<Result<IReadOnlyList<NotificationRecordDto>>>(query, ct);
            return result.ToHttpResult();
        })
        .WithName("GetNotificationsByOrderId")
        .WithSummary("Get all notifications dispatched for a specific order")
        .WithTags("Notifications")
        .Produces<IReadOnlyList<NotificationRecordDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        app.MapGet("/api/notifications/{id:guid}", async (
            Guid id,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new QueryGetNotificationById(id);
            var result = await bus.InvokeAsync<Result<NotificationRecordDto>>(query, ct);
            return result.ToHttpResult();
        })
        .WithName("GetNotificationById")
        .WithSummary("Get a notification by ID")
        .WithTags("Notifications")
        .Produces<NotificationRecordDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
