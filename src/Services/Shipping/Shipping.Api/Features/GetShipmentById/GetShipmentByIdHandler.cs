using BuildingBlocks.Common.Results;
using Microsoft.EntityFrameworkCore;
using Shipping.Api.Data;
using Shipping.Api.Data.Extensions;
using Shipping.Api.Domain;
using Shipping.Api.Domain.Errors;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Queries;

namespace Shipping.Api.Features.GetShipmentById;

public class GetShipmentByIdHandler
{
    private readonly ShippingDbContext _dbContext;

    public GetShipmentByIdHandler(ShippingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ShipmentDetailsDto>> Handle(
        QueryGetShipmentById query,
        CancellationToken cancellationToken)
    {
        if (!ShipmentId.TryCreate(query.ShipmentId, out var shipmentId))
        {
            return Error.NotFound("Shipping.NotFound", $"Shipment '{query.ShipmentId}' was not found.");
        }

        var shipment = await _dbContext.Shipments
            .AsNoTracking()
            .IncludeShipmentDetails()
            .FirstOrDefaultAsync(s => s.Id == shipmentId, cancellationToken);

        if (shipment is null)
        {
            return ShippingErrors.ShipmentNotFound(shipmentId);
        }

        return shipment.ToDetailsDto();
    }
}
