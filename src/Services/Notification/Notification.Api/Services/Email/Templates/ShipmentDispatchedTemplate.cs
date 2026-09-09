namespace Notification.Api.Services.Email.Templates;

public static class ShipmentDispatchedTemplate
{
    public static string Render(
        Guid shipmentId,
        Guid orderId,
        string trackingNumber,
        string carrier,
        string labelUrl,
        DateTime dispatchedAtUtc)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>Your Order Has Shipped! #{{orderId}}</title>
          <style>
            body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f6f9fc; color: #333; margin: 0; padding: 20px; }
            .container { max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
            .header { background: #0f766e; color: #ffffff; padding: 32px 24px; text-align: center; }
            .header h1 { margin: 0 0 8px 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px; }
            .header p { margin: 0; color: #ccfbf1; font-size: 14px; }
            .content { padding: 32px 24px; }
            .badge { display: inline-block; background: #ccfbf1; color: #0f766e; font-weight: 600; font-size: 12px; padding: 4px 10px; border-radius: 12px; margin-bottom: 16px; }
            .tracking-card { background: #f0fdfa; border: 1px solid #99f6e4; border-radius: 8px; padding: 20px; margin: 20px 0; text-align: center; }
            .carrier { font-size: 13px; text-transform: uppercase; color: #0d9488; font-weight: 700; margin-bottom: 4px; }
            .tracking-number { font-family: monospace; font-size: 20px; font-weight: 700; color: #115e59; letter-spacing: 1px; margin-bottom: 16px; }
            .button { display: inline-block; background: #0f766e; color: #ffffff; text-decoration: none; padding: 12px 28px; border-radius: 6px; font-weight: 600; font-size: 14px; }
            .dispatched-time { font-size: 13px; color: #64748b; margin-top: 12px; }
            .footer { background: #f8fafc; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header">
              <h1>LOGAN LOGISTICS</h1>
              <p>Great news! Your package is on its way.</p>
            </div>
            <div class="content">
              <span class="badge">Shipment Dispatched</span>
              <p>Your package for <strong>Order #{{orderId}}</strong> has left our fulfillment facility and is now with the carrier.</p>

              <div class="tracking-card">
                <div class="carrier">{{carrier}}</div>
                <div class="tracking-number">{{trackingNumber}}</div>
                <div>
                  <a href="{{labelUrl}}" class="button">View Tracking & Label</a>
                </div>
                <div class="dispatched-time">Dispatched on {{dispatchedAtUtc:yyyy-MM-dd HH:mm:ss}} UTC</div>
              </div>

              <p style="font-size: 14px; color: #64748b;">
                Tracking events may take up to 24 hours to appear on the carrier network.
              </p>
            </div>
            <div class="footer">
              Logan Fulfillment Center &bull; Tracking Reference ID: {{shipmentId}}
            </div>
          </div>
        </body>
        </html>
        """;
    }
}
