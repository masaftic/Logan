using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Payment.Contracts.Commands;
using Wolverine;

namespace Payment.Api.Features.RefundPayment;

public static class RefundPaymentEndpoint
{
    public static void MapRefundPaymentEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/payments/orders/{orderId:guid}/refund", async (
            Guid orderId,
            [FromBody] RefundOrderPaymentRequest request,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var command = new RefundPaymentCommand(orderId, request.Amount, request.Reason, request.PaymentId);
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToHttpResult();
        })
        .WithName("RefundOrderPayment")
        .WithSummary("Refund payment for an order")
        .WithTags("Payments")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        app.MapPost("/api/payments/refund", async (
            [FromBody] RefundPaymentCommand command,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToHttpResult();
        })
        .WithName("RefundPayment")
        .WithSummary("Refund a payment")
        .WithTags("Payments")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}

public record RefundOrderPaymentRequest(
    decimal Amount,
    string Reason,
    Guid? PaymentId = null
);
