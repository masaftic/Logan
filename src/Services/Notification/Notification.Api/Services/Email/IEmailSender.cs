namespace Notification.Api.Services.Email;

public interface IEmailSender
{
    Task SendEmailAsync(string recipient, string subject, string htmlBody, CancellationToken ct = default);
}
