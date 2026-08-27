using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Inventory.Contracts.Commands;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Inventory.Api.Features.ConfirmStockDeduction;

public static class ConfirmStockDeductionEndpoint
{
    public static void MapConfirmStockDeductionEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/inventory/reservations/confirm", async (
            [FromBody] ConfirmStockDeductionCommand command,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            var result = await bus.InvokeAsync<Result>(command, ct);
            return result.ToAcceptedResult();
        })
        .WithName("ConfirmStockDeduction")
        .WithSummary("Confirm stock deduction for a fulfilled order")
        .WithTags("Inventory");
    }
}

