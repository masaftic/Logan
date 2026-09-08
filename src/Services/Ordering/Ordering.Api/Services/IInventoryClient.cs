using BuildingBlocks.Common.Results;
using Inventory.Contracts.Commands;

namespace Ordering.Api.Services;

public interface IInventoryClient
{
    Task<Result> ReserveStockAsync(ReserveStockCommand command, CancellationToken ct = default);
}
