namespace Notification.Api.Services.Email;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string SenderName { get; set; } = "Logan Commerce";
    public string SenderEmail { get; set; } = "notifications@logan.local";
}
