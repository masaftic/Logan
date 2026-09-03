using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Payment.Contracts.DTOs;
using Payment.Contracts.Queries;
using Wolverine;

namespace Payment.Api.Features.GetPayment;

public static class GetPaymentByOrderIdEndpoint
{
    public static void MapGetPaymentByOrderIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/payments/orders/{orderId:guid}", async (
            Guid orderId,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var query = new GetPaymentByOrderIdQuery(orderId);
            var result = await bus.InvokeAsync<Result<PaymentRecordDto>>(query, ct);
            return result.ToHttpResult();
        })
        .WithName("GetPaymentByOrderId")
        .WithSummary("Get payment details for an order")
        .WithTags("Payments")
        .Produces<PaymentRecordDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
