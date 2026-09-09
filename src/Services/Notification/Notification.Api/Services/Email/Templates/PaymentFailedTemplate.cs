namespace Notification.Api.Services.Email.Templates;

public static class PaymentFailedTemplate
{
    public static string Render(
        Guid orderId,
        string errorCode,
        string declineReason,
        DateTime failedAtUtc)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>Payment Action Required: Order #{{orderId}}</title>
          <style>
            body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f6f9fc; color: #333; margin: 0; padding: 20px; }
            .container { max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
            .header { background: #b91c1c; color: #ffffff; padding: 32px 24px; text-align: center; }
            .header h1 { margin: 0 0 8px 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px; }
            .header p { margin: 0; color: #fecaca; font-size: 14px; }
            .content { padding: 32px 24px; }
            .badge { display: inline-block; background: #fee2e2; color: #991b1b; font-weight: 600; font-size: 12px; padding: 4px 10px; border-radius: 12px; margin-bottom: 16px; }
            .alert-box { background: #fef2f2; border: 1px solid #f87171; border-radius: 6px; padding: 16px; margin: 20px 0; font-size: 14px; line-height: 1.6; }
            .details { font-size: 14px; line-height: 1.6; margin-bottom: 24px; }
            .button { display: inline-block; background: #b91c1c; color: #ffffff; text-decoration: none; padding: 12px 24px; border-radius: 6px; font-weight: 600; font-size: 14px; }
            .footer { background: #f8fafc; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header">
              <h1>LOGAN COMMERCE</h1>
              <p>Payment Processing Alert</p>
            </div>
            <div class="content">
              <span class="badge">Payment Failed</span>
              <div class="details">
                We were unable to process your payment for <strong>Order #{{orderId}}</strong> on {{failedAtUtc:yyyy-MM-dd HH:mm:ss}} UTC.
              </div>

              <div class="alert-box">
                <strong>Decline Reason:</strong> {{declineReason}}<br/>
                <strong>Error Code:</strong> <code>{{errorCode}}</code>
              </div>

              <div class="details">
                Don't worry, your order items are temporarily reserved. Please update your payment method or retry the transaction to prevent cancellation.
              </div>

              <div style="text-align: center; margin-top: 24px;">
                <a href="https://logan.local/checkout/retry?orderId={{orderId}}" class="button">Retry Payment</a>
              </div>
            </div>
            <div class="footer">
              If you did not initiate this payment, please contact security support at security@logan.local.
            </div>
          </div>
        </body>
        </html>
        """;
    }
}
