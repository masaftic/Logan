namespace Notification.Api.Services.Sms;

public interface ISmsSender
{
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default);
}
