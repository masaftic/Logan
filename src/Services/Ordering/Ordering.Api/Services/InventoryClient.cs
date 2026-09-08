using BuildingBlocks.Common.Extensions;
using BuildingBlocks.Common.Results;
using Inventory.Contracts.Commands;

namespace Ordering.Api.Services;

public class InventoryClient(HttpClient httpClient) : IInventoryClient
{
    public Task<Result> ReserveStockAsync(ReserveStockCommand command, CancellationToken ct = default) =>
        httpClient.PostAsJsonResultAsync("/api/inventory/reservations", command, ct);
}
