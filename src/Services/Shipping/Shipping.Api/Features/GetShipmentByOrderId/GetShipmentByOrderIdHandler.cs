using BuildingBlocks.Common.Results;
using Microsoft.EntityFrameworkCore;
using Shipping.Api.Data;
using Shipping.Api.Data.Extensions;
using Shipping.Api.Domain.Errors;
using Shipping.Contracts.DTOs;
using Shipping.Contracts.Queries;

namespace Shipping.Api.Features.GetShipmentByOrderId;

public class GetShipmentByOrderIdHandler
{
    private readonly ShippingDbContext _dbContext;

    public GetShipmentByOrderIdHandler(ShippingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ShipmentDetailsDto>> Handle(
        QueryGetShipmentByOrderId query,
        CancellationToken cancellationToken)
    {
        var shipment = await _dbContext.Shipments
            .AsNoTracking()
            .IncludeShipmentDetails()
            .Where(s => s.OrderId == query.OrderId)
            .OrderByDescending(s => s.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (shipment is null)
        {
            return ShippingErrors.ShipmentNotFoundForOrder(query.OrderId);
        }

        return shipment.ToDetailsDto();
    }
}
