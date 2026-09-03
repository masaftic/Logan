using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Common.Results;
using Ordering.Contracts.Commands;
using Ordering.Contracts.DTOs;
using Payment.Contracts.Commands;
using Payment.Contracts.DTOs;

namespace ApiGateway.Features.Checkout;

public static class CheckoutJsonSerializerOptions
{
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}

public interface IOrderingClient
{
    Task<Result<OrderDto>> SubmitOrderAsync(SubmitOrderCommand command, CancellationToken ct);
}

public class OrderingClient(HttpClient httpClient) : IOrderingClient
{
    public async Task<Result<OrderDto>> SubmitOrderAsync(SubmitOrderCommand command, CancellationToken ct)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/orders", command, CheckoutJsonSerializerOptions.Default, ct);
            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadAsStringAsync(ct);
                return Error.ExternalService("Ordering.Error", $"Ordering service error: {problem}");
            }

            var order = await response.Content.ReadFromJsonAsync<OrderDto>(CheckoutJsonSerializerOptions.Default, ct);
            return order is not null
                ? Result<OrderDto>.Ok(order)
                : Error.Unexpected("Ordering.InvalidResponse", "Received null order from Ordering service.");
        }
        catch (Exception ex)
        {
            return Error.ExternalService("Ordering.Unavailable", $"Failed to communicate with Ordering service: {ex.Message}");
        }
    }
}

public interface IPaymentClient
{
    Task<Result<PaymentInitializationDto>> InitializePaymentAsync(InitializePaymentCommand command, CancellationToken ct);
}

public class PaymentClient(HttpClient httpClient) : IPaymentClient
{
    public async Task<Result<PaymentInitializationDto>> InitializePaymentAsync(InitializePaymentCommand command, CancellationToken ct)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync("/api/payments/initialize", command, CheckoutJsonSerializerOptions.Default, ct);
            if (!response.IsSuccessStatusCode)
            {
                var problem = await response.Content.ReadAsStringAsync(ct);
                return Error.ExternalService("Payment.Error", $"Payment service error: {problem}");
            }

            var payment = await response.Content.ReadFromJsonAsync<PaymentInitializationDto>(CheckoutJsonSerializerOptions.Default, ct);
                
            return payment is not null
                ? Result<PaymentInitializationDto>.Ok(payment)
                : Error.Unexpected("Payment.InvalidResponse", "Received null payment from Payment service.");
        }
        catch (Exception ex)
        {
            return Error.ExternalService("Payment.Unavailable", $"Failed to communicate with Payment service: {ex.Message}");
        }
    }
}
