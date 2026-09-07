using FluentValidation;
using Shipping.Contracts.Queries;

namespace Shipping.Api.Features.GetShipmentByOrderId;

public class GetShipmentByOrderIdQueryValidator : AbstractValidator<QueryGetShipmentByOrderId>
{
    public GetShipmentByOrderIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId cannot be empty.");
    }
}
