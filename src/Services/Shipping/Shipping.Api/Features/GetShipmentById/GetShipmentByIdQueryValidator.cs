using FluentValidation;
using Shipping.Contracts.Queries;

namespace Shipping.Api.Features.GetShipmentById;

public class GetShipmentByIdQueryValidator : AbstractValidator<QueryGetShipmentById>
{
    public GetShipmentByIdQueryValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty()
            .WithMessage("ShipmentId cannot be empty.");
    }
}
