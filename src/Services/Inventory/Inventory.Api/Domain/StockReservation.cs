using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Inventory.Api.Domain.Enums;
using Inventory.Api.Domain.Errors;

namespace Inventory.Api.Domain;

public class StockReservation
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Sku { get; private set; } = null!;
    public PositiveQuantity Quantity { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public DateTime? ResolvedAtUtc { get; private set; }

    private StockReservation() { }

    public static Result<StockReservation> Create(
        Guid orderId,
        string sku,
        PositiveQuantity quantity,
        TimeSpan? holdDuration = null)
    {
        var now = DateTime.UtcNow;
        return new StockReservation
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Sku = sku,
            Quantity = quantity,
            Status = ReservationStatus.Active,
            CreatedAtUtc = now,
            ExpiresAtUtc = holdDuration.HasValue ? now.Add(holdDuration.Value) : null
        };
    }

    public Result Confirm()
    {
        if (Status != ReservationStatus.Active)
        {
            return InventoryErrors.InvalidReservationState(Id, Status, nameof(Confirm));
        }

        Status = ReservationStatus.Confirmed;
        ResolvedAtUtc = DateTime.UtcNow;
        return Result.Ok();
    }

    public Result Release()
    {
        if (Status != ReservationStatus.Active)
        {
            return InventoryErrors.InvalidReservationState(Id, Status, nameof(Release));
        }

        Status = ReservationStatus.Released;
        ResolvedAtUtc = DateTime.UtcNow;
        return Result.Ok();
    }
}
