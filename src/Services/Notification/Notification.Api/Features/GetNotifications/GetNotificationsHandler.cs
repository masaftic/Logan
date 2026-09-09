using BuildingBlocks.Common.Results;
using Microsoft.EntityFrameworkCore;
using Notification.Api.Data;
using Notification.Api.Domain;
using Notification.Contracts.DTOs;
using Notification.Contracts.Enums;
using Notification.Contracts.Queries;

namespace Notification.Api.Features.GetNotifications;

public static class GetNotificationsHandler
{
    public static async Task<Result<IReadOnlyList<NotificationRecordDto>>> Handle(
        QueryGetNotificationsByOrderId query,
        NotificationDbContext dbContext,
        CancellationToken ct)
    {
        var records = await dbContext.Notifications
            .AsNoTracking()
            .Where(n => n.ReferenceId == query.OrderId)
            .OrderByDescending(n => n.CreatedAtUtc)
            .ToListAsync(ct);

        var dtos = records.Select(MapToDto).ToList();
        return dtos;
    }

    public static async Task<Result<NotificationRecordDto>> Handle(
        QueryGetNotificationById query,
        NotificationDbContext dbContext,
        CancellationToken ct)
    {
        var record = await dbContext.Notifications
            .AsNoTracking()
            .SingleOrDefaultAsync(n => n.Id == query.NotificationId, ct);

        if (record is null)
        {
            return Error.NotFound("Notification.NotFound", $"Notification with ID '{query.NotificationId}' was not found.");
        }

        return MapToDto(record);
    }

    private static NotificationRecordDto MapToDto(NotificationRecord entity) =>
        new(
            entity.Id,
            entity.ReferenceId,
            (NotificationTypeDto)entity.NotificationType,
            (NotificationChannelDto)entity.Channel,
            entity.Recipient,
            entity.Subject,
            entity.Body,
            (NotificationStatusDto)entity.Status,
            entity.SentAtUtc,
            entity.ErrorMessage,
            entity.CreatedAtUtc
        );
}
