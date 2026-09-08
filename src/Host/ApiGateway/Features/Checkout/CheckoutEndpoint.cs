using BuildingBlocks.Common.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Ordering.Contracts.Commands;
using Payment.Contracts.Commands;
using Payment.Contracts.DTOs;
using Payment.Contracts.Enums;

namespace ApiGateway.Features.Checkout;

public static class CheckoutEndpoint
{
    public static void MapCheckoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/checkout", async (
            [FromBody] CheckoutRequest request,
            IOrderingClient orderingClient,
            IPaymentClient paymentClient,
            CancellationToken ct) =>
        {
            var submitOrderCommand = new SubmitOrderCommand(
                request.CustomerId,
                request.Items,
                request.Currency,
                request.ProviderRateId,
                request.DestinationAddress,
                request.Dimensions,
                request.Weight);
            var orderResult = await orderingClient.SubmitOrderAsync(submitOrderCommand, ct);

            if (orderResult.IsError)
            {
                return orderResult.ToHttpResult();
            }

            var order = orderResult.Value;

            var initPaymentCommand = new InitializePaymentCommand(
                order.Id,
                order.CustomerId,
                order.TotalAmount,
                request.Currency
            );

            var paymentResult = await paymentClient.InitializePaymentAsync(initPaymentCommand, ct);

            PaymentInitializationDto paymentDto;
            if (paymentResult.IsSuccess)
            {
                paymentDto = paymentResult.Value;
            }
            else
            {
                paymentDto = new PaymentInitializationDto(
                    OrderId: order.Id,
                    Status: PaymentStatusDto.InitializationFailed,
                    ClientSecret: null,
                    PaymentIntentId: null,
                    Amount: order.TotalAmount,
                    Currency: request.Currency,
                    ErrorMessage: paymentResult.FirstError.Description
                );
            }

            return Results.Ok(new CheckoutResponse(order, paymentDto));
        })
        .WithName("Checkout")
        .WithSummary("Orchestrate checkout by creating order and initializing payment intent")
        .WithTags("Checkout")
        .Produces<CheckoutResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
