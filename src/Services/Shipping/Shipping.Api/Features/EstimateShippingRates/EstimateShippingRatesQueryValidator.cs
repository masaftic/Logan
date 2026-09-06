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

        RuleFor(x => x.Dimensions)
            .NotNull()
            .WithMessage("Dimensions are required.");

        When(x => x.Dimensions is not null, () =>
        {
            RuleFor(x => x.Dimensions.Length)
                .GreaterThan(0)
                .WithMessage("Length must be greater than zero.");

            RuleFor(x => x.Dimensions.Width)
                .GreaterThan(0)
                .WithMessage("Width must be greater than zero.");

            RuleFor(x => x.Dimensions.Height)
                .GreaterThan(0)
                .WithMessage("Height must be greater than zero.");

            RuleFor(x => x.Dimensions.Unit)
                .NotEmpty()
                .WithMessage("Dimension unit is required.");
        });

        RuleFor(x => x.Weight)
            .NotNull()
            .WithMessage("Weight is required.");

        When(x => x.Weight is not null, () =>
        {
            RuleFor(x => x.Weight.Value)
                .GreaterThan(0)
                .WithMessage("Weight value must be greater than zero.");

            RuleFor(x => x.Weight.Unit)
                .NotEmpty()
                .WithMessage("Weight unit is required.");
        });
    }
}
