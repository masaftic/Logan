using Microsoft.EntityFrameworkCore;
using Notification.Api.Data;
using Notification.Api.Domain;
using Notification.Api.Domain.Enums;
using Notification.Api.Services.Email;
using Notification.Api.Services.Email.Templates;
using Notification.Api.Services.Sms;
using Shipping.Contracts.Events;

namespace Notification.Api.Features.ProcessShipmentDispatched;

public class ProcessShipmentDispatchedHandler
{
    private readonly NotificationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly ILogger<ProcessShipmentDispatchedHandler> _logger;

    public ProcessShipmentDispatchedHandler(
        NotificationDbContext dbContext,
        IEmailSender emailSender,
        ISmsSender smsSender,
        ILogger<ProcessShipmentDispatchedHandler> logger)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logger = logger;
    }

    public async Task Handle(ShipmentLabelPurchasedEvent @event, CancellationToken ct)
    {
        var emailAlreadySent = await _dbContext.Notifications.AnyAsync(
            n => n.ReferenceId == @event.OrderId &&
                 n.NotificationType == NotificationType.ShipmentDispatched &&
                 n.Channel == NotificationChannel.Email,
            ct);

        if (!emailAlreadySent)
        {
            var recipient = $"customer-order-{@event.OrderId.ToString()[..8]}@example.com";
            var subject = $"Your Order Has Shipped! - Order #{@event.OrderId}";
            var htmlBody = ShipmentDispatchedTemplate.Render(
                @event.ShipmentId,
                @event.OrderId,
                @event.TrackingNumber,
                @event.Carrier,
                @event.LabelUrl,
                @event.DispatchedAtUtc);

            try
            {
                await _emailSender.SendEmailAsync(recipient, subject, htmlBody, ct);

                var emailRecord = NotificationRecord.CreateSent(
                    referenceId: @event.OrderId,
                    type: NotificationType.ShipmentDispatched,
                    channel: NotificationChannel.Email,
                    recipient: recipient,
                    subject: subject,
                    body: htmlBody,
                    sentAtUtc: DateTime.UtcNow);

                _dbContext.Notifications.Add(emailRecord);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogInformation("Successfully sent shipment dispatched email for Order {OrderId}.", @event.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver shipment dispatch email for Order {OrderId}.", @event.OrderId);
                var failedRecord = NotificationRecord.CreatePending(
                    referenceId: @event.OrderId,
                    type: NotificationType.ShipmentDispatched,
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

        var smsAlreadySent = await _dbContext.Notifications.AnyAsync(
            n => n.ReferenceId == @event.OrderId &&
                 n.NotificationType == NotificationType.ShipmentDispatched &&
                 n.Channel == NotificationChannel.Sms,
            ct);

        if (!smsAlreadySent)
        {
            var phoneNumber = "+15550199";
            var smsText = $"Logan Order #{@event.OrderId.ToString()[..8]} shipped via {@event.Carrier}! Tracking: {@event.TrackingNumber}";

            await _smsSender.SendSmsAsync(phoneNumber, smsText, ct);

            var smsRecord = NotificationRecord.CreateSent(
                referenceId: @event.OrderId,
                type: NotificationType.ShipmentDispatched,
                channel: NotificationChannel.Sms,
                recipient: phoneNumber,
                subject: "Shipment SMS",
                body: smsText,
                sentAtUtc: DateTime.UtcNow);

            _dbContext.Notifications.Add(smsRecord);
            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Successfully recorded shipment dispatched SMS for Order {OrderId}.", @event.OrderId);
        }
    }
}
