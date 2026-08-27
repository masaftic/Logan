using FluentValidation;
using Inventory.Contracts.Commands;

namespace Inventory.Api.Features.RestockItem;

public class RestockItemCommandValidator : AbstractValidator<RestockItemCommand>
{
    public RestockItemCommandValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");
    }
}

