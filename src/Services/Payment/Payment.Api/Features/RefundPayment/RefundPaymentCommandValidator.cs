using FluentValidation;
using Payment.Contracts.Commands;

namespace Payment.Api.Features.RefundPayment;

public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Refund reason is required.");
    }
}
