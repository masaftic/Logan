using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Notification.Api.Services.Email;

public class MailpitSmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<MailpitSmtpEmailSender> _logger;

    public MailpitSmtpEmailSender(
        IOptions<SmtpOptions> options,
        ILogger<MailpitSmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string recipient, string subject, string htmlBody, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            _logger.LogInformation("Connecting to SMTP server at {Host}:{Port}...", _options.Host, _options.Port);
            await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.None, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Successfully dispatched email '{Subject}' to {Recipient}.", subject, recipient);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email '{Subject}' to {Recipient} via {Host}:{Port}.",
                subject, recipient, _options.Host, _options.Port);
            throw;
        }
    }
}
