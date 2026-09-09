namespace Notification.Api.Services.Sms;

public class LoggingSmsSender : ISmsSender
{
    private readonly ILogger<LoggingSmsSender> _logger;

    public LoggingSmsSender(ILogger<LoggingSmsSender> logger)
    {
        _logger = logger;
    }

    public Task SendSmsAsync(string phoneNumber, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("[SMS DISPATCH] To: {PhoneNumber} | Message: {Message}", phoneNumber, message);
        return Task.CompletedTask;
    }
}
