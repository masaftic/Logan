using FluentValidation;
using Shipping.Contracts.Commands;

namespace Shipping.Api.Features.ShippoWebhook;

public class CommandProcessShippoWebhookValidator : AbstractValidator<CommandProcessShippoWebhook>
{
    public CommandProcessShippoWebhookValidator()
    {
        RuleFor(x => x.TrackingNumber)
            .NotEmpty()
            .WithMessage("Tracking number is required.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.");
    }
}
