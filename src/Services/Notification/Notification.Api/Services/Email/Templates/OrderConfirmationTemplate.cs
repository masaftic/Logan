using System.Text;
using Ordering.Contracts.DTOs;
using Shipping.Contracts.DTOs;

namespace Notification.Api.Services.Email.Templates;

public static class OrderConfirmationTemplate
{
    public static string Render(
        Guid orderId,
        Guid customerId,
        decimal totalAmount,
        string currency,
        IReadOnlyList<OrderItemDto> items,
        ShippingAddressDto address)
    {
        var sb = new StringBuilder();
        sb.Append($$"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8">
          <meta name="viewport" content="width=device-width, initial-scale=1.0">
          <title>Order Confirmation #{{orderId}}</title>
          <style>
            body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f6f9fc; color: #333; margin: 0; padding: 20px; }
            .container { max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
            .header { background: #1a1a2e; color: #ffffff; padding: 32px 24px; text-align: center; }
            .header h1 { margin: 0 0 8px 0; font-size: 24px; font-weight: 700; letter-spacing: -0.5px; }
            .header p { margin: 0; color: #a0a0b0; font-size: 14px; }
            .content { padding: 32px 24px; }
            .greeting { font-size: 16px; margin-bottom: 24px; }
            .badge { display: inline-block; background: #e8f5e9; color: #2e7d32; font-weight: 600; font-size: 12px; padding: 4px 10px; border-radius: 12px; margin-bottom: 16px; }
            table { width: 100%; border-collapse: collapse; margin-bottom: 24px; }
            th { text-align: left; font-size: 12px; text-transform: uppercase; color: #888; border-bottom: 2px solid #eee; padding: 8px 4px; }
            td { padding: 12px 4px; border-bottom: 1px solid #f0f0f0; font-size: 14px; }
            .total-row td { font-weight: 700; font-size: 16px; border-top: 2px solid #333; border-bottom: none; }
            .section-title { font-size: 14px; text-transform: uppercase; color: #777; font-weight: 700; margin: 24px 0 8px 0; }
            .address-box { background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; padding: 16px; font-size: 14px; line-height: 1.6; }
            .footer { background: #f8fafc; padding: 20px; text-align: center; font-size: 12px; color: #94a3b8; border-top: 1px solid #e2e8f0; }
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header">
              <h1>LOGAN COMMERCE</h1>
              <p>Thank you for your purchase!</p>
            </div>
            <div class="content">
              <span class="badge">Payment Confirmed & Order Placed</span>
              <div class="greeting">
                Hello <strong>{{(string.IsNullOrWhiteSpace(address.Name) ? "Customer" : address.Name)}}</strong>,<br>
                We've confirmed your payment and your order is now being prepared for fulfillment.
              </div>

              <div class="section-title">Order Summary (ID: {{orderId}})</div>
              <table>
                <thead>
                  <tr>
                    <th>Item SKU</th>
                    <th style="text-align: center;">Qty</th>
                    <th style="text-align: right;">Unit Price</th>
                    <th style="text-align: right;">Total</th>
                  </tr>
                </thead>
                <tbody>
        """);

        foreach (var item in items)
        {
            sb.Append($$"""
                  <tr>
                    <td><code>{{item.Sku}}</code></td>
                    <td style="text-align: center;">{{item.Quantity}}</td>
                    <td style="text-align: right;">{{item.UnitPrice:F2}} {{currency}}</td>
                    <td style="text-align: right;">{{item.TotalPrice:F2}} {{currency}}</td>
                  </tr>
            """);
        }

        sb.Append($$"""
                  <tr class="total-row">
                    <td colspan="3">Total Amount</td>
                    <td style="text-align: right;">{{totalAmount:F2}} {{currency}}</td>
                  </tr>
                </tbody>
              </table>

              <div class="section-title">Shipping Destination</div>
              <div class="address-box">
                {{(string.IsNullOrWhiteSpace(address.Name) ? "" : $"{address.Name}<br/>")}}
                {{address.Street1}} {{(string.IsNullOrWhiteSpace(address.Street2) ? "" : address.Street2)}}<br/>
                {{address.City}}, {{address.State}} {{address.PostalCode}}<br/>
                {{address.Country}}
                {{(string.IsNullOrWhiteSpace(address.Phone) ? "" : $"<br/>Phone: {address.Phone}")}}
              </div>
            </div>
            <div class="footer">
              This is a transactional confirmation from the Logan Distributed Fulfillment Engine.
            </div>
          </div>
        </body>
        </html>
        """);

        return sb.ToString();
    }
}
