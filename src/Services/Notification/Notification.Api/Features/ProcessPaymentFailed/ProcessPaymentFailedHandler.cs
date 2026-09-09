using Microsoft.EntityFrameworkCore;
using Notification.Api.Data;
using Notification.Api.Domain;
using Notification.Api.Domain.Enums;
using Notification.Api.Services.Email;
using Notification.Api.Services.Email.Templates;
using Payment.Contracts.Events;

namespace Notification.Api.Features.ProcessPaymentFailed;

public class ProcessPaymentFailedHandler
{
    private readonly NotificationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ProcessPaymentFailedHandler> _logger;

    public ProcessPaymentFailedHandler(
        NotificationDbContext dbContext,
        IEmailSender emailSender,
        ILogger<ProcessPaymentFailedHandler> logger)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Handle(PaymentFailedEvent @event, CancellationToken ct)
    {
        var alreadySent = await _dbContext.Notifications.AnyAsync(
            n => n.ReferenceId == @event.OrderId &&
                 n.NotificationType == NotificationType.PaymentFailedAlert &&
                 n.Channel == NotificationChannel.Email,
            ct);

        if (alreadySent)
        {
            _logger.LogInformation("Payment failed alert already processed for Order {OrderId}. Skipping duplicate.", @event.OrderId);
            return;
        }

        var recipient = $"customer-order-{@event.OrderId.ToString()[..8]}@example.com";
        var subject = $"Payment Action Required - Order #{@event.OrderId}";
        var htmlBody = PaymentFailedTemplate.Render(
            @event.OrderId,
            @event.ErrorCode,
            @event.DeclineReason,
            @event.FailedAtUtc);

        try
        {
            await _emailSender.SendEmailAsync(recipient, subject, htmlBody, ct);

            var record = NotificationRecord.CreateSent(
                referenceId: @event.OrderId,
                type: NotificationType.PaymentFailedAlert,
                channel: NotificationChannel.Email,
                recipient: recipient,
                subject: subject,
                body: htmlBody,
                sentAtUtc: DateTime.UtcNow);

            _dbContext.Notifications.Add(record);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully recorded payment failed notification for Order {OrderId}.", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver payment failed email for Order {OrderId}.", @event.OrderId);

            var failedRecord = NotificationRecord.CreatePending(
                referenceId: @event.OrderId,
                type: NotificationType.PaymentFailedAlert,
                channel: NotificationChannel.Email,
                recipient: recipient,
                subject: subject,
                body: htmlBody);

            failedRecord.MarkFailed(ex.Message);
            _dbContext.Notifications.Add(failedRecord);
            await _dbContext.SaveChangesAsync(ct);
            throw;
        }
    }
}
