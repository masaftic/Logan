using Notification.Contracts.Enums;

namespace Notification.Contracts.DTOs;

public record NotificationRecordDto(
    Guid Id,
    Guid ReferenceId,
    NotificationTypeDto NotificationType,
    NotificationChannelDto Channel,
    string Recipient,
    string Subject,
    string Body,
    NotificationStatusDto Status,
    DateTime? SentAtUtc,
    string? ErrorMessage,
    DateTime CreatedAtUtc
);
