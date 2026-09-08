using FluentValidation;
using Shipping.Contracts.Queries;

namespace Shipping.Api.Features.EstimateShippingRates;

public class EstimateShippingRatesQueryValidator : AbstractValidator<QueryShippingRates>
{
    public EstimateShippingRatesQueryValidator()
    {
        RuleFor(x => x.DestinationAddress)
            .NotNull()
            .WithMessage("Destination address is required.");

        When(x => x.DestinationAddress is not null, () =>
        {
            RuleFor(x => x.DestinationAddress.Street1)
                .NotEmpty()
                .WithMessage("Street1 is required.");

            RuleFor(x => x.DestinationAddress.City)
                .NotEmpty()
                .WithMessage("City is required.");

            RuleFor(x => x.DestinationAddress.State)
                .NotEmpty()
                .WithMessage("State is required.");

            RuleFor(x => x.DestinationAddress.PostalCode)
                .NotEmpty()
                .WithMessage("Postal code is required.");

            RuleFor(x => x.DestinationAddress.Country)
                .NotEmpty()
                .Length(2)
                .WithMessage("Country must be a 2-letter ISO country code.");
        });

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one shipment item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Sku)
                .NotEmpty()
                .WithMessage("Item SKU is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage("Item quantity must be greater than zero.");
        });
    }
}
