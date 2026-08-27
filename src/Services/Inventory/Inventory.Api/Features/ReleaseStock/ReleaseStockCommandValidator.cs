using FluentValidation;
using Inventory.Contracts.Commands;

namespace Inventory.Api.Features.ReleaseStock;

public class ReleaseStockCommandValidator : AbstractValidator<ReleaseStockCommand>
{
    public ReleaseStockCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");
    }
}

