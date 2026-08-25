using Api.Features.Payments.CreatePaymentIntent;
using Api.Features.Payments.MarkPaymentPaid;

namespace Api.Features.Payments;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api")
            .WithTags("Payments");

        group.MapCreatePaymentIntentEndpoint();
        group.MapMarkPaymentPaidEndpoint();

        return app;
    }
}
