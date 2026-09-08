using FluentValidation;
using Ordering.Contracts.Commands;

namespace Ordering.Api.Features.SubmitOrder;

public class SubmitOrderCommandValidator : AbstractValidator<SubmitOrderCommand>
{
    public SubmitOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("CustomerId is required.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Sku)
                .NotEmpty()
                .WithMessage("SKU is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        });

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.ProviderRateId)
            .NotEmpty()
            .WithMessage("ProviderRateId is required.");

        RuleFor(x => x.DestinationAddress)
            .NotNull()
            .WithMessage("DestinationAddress is required.");

        When(x => x.DestinationAddress is not null, () =>
        {
            RuleFor(x => x.DestinationAddress.Street1).NotEmpty().WithMessage("Street1 is required.");
            RuleFor(x => x.DestinationAddress.City).NotEmpty().WithMessage("City is required.");
            RuleFor(x => x.DestinationAddress.State).NotEmpty().WithMessage("State is required.");
            RuleFor(x => x.DestinationAddress.PostalCode).NotEmpty().WithMessage("PostalCode is required.");
            RuleFor(x => x.DestinationAddress.Country).NotEmpty().WithMessage("Country is required.");
        });
    }
}
