using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Ordering.Contracts.Commands;
using Ordering.Contracts.DTOs;
using Payment.Contracts.Commands;
using Payment.Contracts.DTOs;

namespace ApiGateway.Features.Checkout;

public interface IOrderingClient
{
    Task<Result<OrderDto>> SubmitOrderAsync(SubmitOrderCommand command, CancellationToken ct);
}

public class OrderingClient(HttpClient httpClient) : IOrderingClient
{
    public Task<Result<OrderDto>> SubmitOrderAsync(SubmitOrderCommand command, CancellationToken ct) =>
        httpClient.PostAsJsonResultAsync<SubmitOrderCommand, OrderDto>("/api/orders", command, ct);
}

public interface IPaymentClient
{
    Task<Result<PaymentInitializationDto>> InitializePaymentAsync(InitializePaymentCommand command, CancellationToken ct);
}

public class PaymentClient(HttpClient httpClient) : IPaymentClient
{
    public Task<Result<PaymentInitializationDto>> InitializePaymentAsync(InitializePaymentCommand command, CancellationToken ct) =>
        httpClient.PostAsJsonResultAsync<InitializePaymentCommand, PaymentInitializationDto>("/api/payments/initialize", command, ct);
}
