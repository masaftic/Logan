using Microsoft.EntityFrameworkCore;
using Notification.Api.Data;
using Notification.Api.Domain;
using Notification.Api.Domain.Enums;
using Notification.Api.Services.Email;
using Notification.Api.Services.Email.Templates;
using Ordering.Contracts.Events;

namespace Notification.Api.Features.ProcessOrderConfirmed;

public class ProcessOrderConfirmedHandler
{
    private readonly NotificationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ProcessOrderConfirmedHandler> _logger;

    public ProcessOrderConfirmedHandler(
        NotificationDbContext dbContext,
        IEmailSender emailSender,
        ILogger<ProcessOrderConfirmedHandler> logger)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Handle(OrderConfirmedEvent @event, CancellationToken ct)
    {
        var alreadySent = await _dbContext.Notifications.AnyAsync(
            n => n.ReferenceId == @event.OrderId &&
                 n.NotificationType == NotificationType.OrderConfirmation &&
                 n.Channel == NotificationChannel.Email,
            ct);

        if (alreadySent)
        {
            _logger.LogInformation("Order confirmation email already processed for Order {OrderId}. Skipping duplicate.", @event.OrderId);
            return;
        }

        var recipient = $"customer-{@event.CustomerId.ToString()[..8]}@example.com";
        var subject = $"Order Confirmed - Order #{@event.OrderId}";
        var htmlBody = OrderConfirmationTemplate.Render(
            @event.OrderId,
            @event.CustomerId,
            @event.TotalAmount,
            @event.Currency,
            @event.Items,
            @event.DestinationAddress);

        try
        {
            await _emailSender.SendEmailAsync(recipient, subject, htmlBody, ct);

            var record = NotificationRecord.CreateSent(
                referenceId: @event.OrderId,
                type: NotificationType.OrderConfirmation,
                channel: NotificationChannel.Email,
                recipient: recipient,
                subject: subject,
                body: htmlBody,
                sentAtUtc: DateTime.UtcNow);

            _dbContext.Notifications.Add(record);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully recorded order confirmation notification for Order {OrderId}.", @event.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deliver order confirmation email for Order {OrderId}.", @event.OrderId);

            var failedRecord = NotificationRecord.CreatePending(
                referenceId: @event.OrderId,
                type: NotificationType.OrderConfirmation,
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
