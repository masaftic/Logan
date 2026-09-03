using FluentValidation;
using Payment.Contracts.Queries;

namespace Payment.Api.Features.GetPayment;

public class GetPaymentByOrderIdQueryValidator : AbstractValidator<GetPaymentByOrderIdQuery>
{
    public GetPaymentByOrderIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");
    }
}
