using BuildingBlocks.Common.Results;

namespace Payment.Api.Domain.Errors;

public static class PaymentErrors
{
    public static Error PaymentNotFound(Guid id) =>
        Error.NotFound(
            code: "Payment.NotFound",
            description: $"Payment '{id}' was not found.")
        .WithMetadata("PaymentId", id);

    public static Error PaymentNotFoundForOrder(Guid orderId) =>
        Error.NotFound(
            code: "Payment.NotFoundForOrder",
            description: $"Payment for order '{orderId}' was not found.")
        .WithMetadata("OrderId", orderId);

    public static Error StripeError(string message) =>
        Error.ExternalService(
            code: "Payment.StripeError",
            description: $"Stripe payment processing error: {message}");

    public static Error CannotRefundUnsettledPayment(Guid paymentId, string currentStatus) =>
        Error.Conflict(
            code: "Payment.CannotRefundUnsettled",
            description: $"Cannot refund payment '{paymentId}' because it is in state '{currentStatus}'. Only settled payments can be refunded.")
        .WithMetadata("PaymentId", paymentId)
        .WithMetadata("CurrentStatus", currentStatus);

    public static Error RefundExceedsRemainingBalance(Guid paymentId, decimal requested, decimal remaining) =>
        Error.Validation(
            code: "Payment.RefundExceedsBalance",
            description: $"Requested refund amount of {requested:C} exceeds the remaining unrefunded balance of {remaining:C} on payment '{paymentId}'.")
        .WithMetadata("PaymentId", paymentId)
        .WithMetadata("RequestedAmount", requested)
        .WithMetadata("RemainingBalance", remaining);

    public static Error InvalidWebhookSignature() =>
        Error.Validation(
            code: "Payment.InvalidWebhookSignature",
            description: "Stripe webhook signature validation failed.");
}
