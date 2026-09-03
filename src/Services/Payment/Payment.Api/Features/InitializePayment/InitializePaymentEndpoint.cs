using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Payment.Contracts.Commands;
using Payment.Contracts.DTOs;
using Wolverine;

namespace Payment.Api.Features.InitializePayment;

public static class InitializePaymentEndpoint
{
    public static void MapInitializePaymentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/initialize", async (
            [FromBody] InitializePaymentCommand command,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result<PaymentInitializationDto>>(command, ct);
            return result.ToHttpResult();
        })
        .WithName("InitializePayment")
        .WithSummary("Initialize payment with Stripe for an order")
        .WithTags("Payments")
        .Produces<PaymentInitializationDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
