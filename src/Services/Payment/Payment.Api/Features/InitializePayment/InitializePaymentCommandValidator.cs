using FluentValidation;
using Payment.Contracts.Commands;

namespace Payment.Api.Features.InitializePayment;

public class InitializePaymentCommandValidator : AbstractValidator<InitializePaymentCommand>
{
    public InitializePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.");
    }
}
