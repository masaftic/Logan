using Notification.Api.Domain.Enums;

namespace Notification.Api.Domain;

public class NotificationRecord
{
    public Guid Id { get; private set; }
    public Guid ReferenceId { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Recipient { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAtUtc { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private NotificationRecord() { }

    public static NotificationRecord CreatePending(
        Guid referenceId,
        NotificationType type,
        NotificationChannel channel,
        string recipient,
        string subject,
        string body)
    {
        return new NotificationRecord
        {
            Id = Guid.CreateVersion7(),
            ReferenceId = referenceId,
            NotificationType = type,
            Channel = channel,
            Recipient = recipient,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public static NotificationRecord CreateSent(
        Guid referenceId,
        NotificationType type,
        NotificationChannel channel,
        string recipient,
        string subject,
        string body,
        DateTime sentAtUtc)
    {
        return new NotificationRecord
        {
            Id = Guid.CreateVersion7(),
            ReferenceId = referenceId,
            NotificationType = type,
            Channel = channel,
            Recipient = recipient,
            Subject = subject,
            Body = body,
            Status = NotificationStatus.Sent,
            SentAtUtc = sentAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkSent(DateTime sentAtUtc)
    {
        Status = NotificationStatus.Sent;
        SentAtUtc = sentAtUtc;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = NotificationStatus.Failed;
        ErrorMessage = errorMessage;
    }
}
