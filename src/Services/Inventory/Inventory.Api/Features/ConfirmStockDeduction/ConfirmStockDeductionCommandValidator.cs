using FluentValidation;
using Inventory.Contracts.Commands;

namespace Inventory.Api.Features.ConfirmStockDeduction;

public class ConfirmStockDeductionCommandValidator : AbstractValidator<ConfirmStockDeductionCommand>
{
    public ConfirmStockDeductionCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");
    }
}

