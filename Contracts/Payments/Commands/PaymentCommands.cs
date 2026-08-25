namespace Contracts.Payments.Commands;

public record CreatePaymentIntentCommand(Guid OrderId);

public record MarkPaymentPaidCommand(
    Guid OrderId, 
    Guid? PaymentId = null, 
    string? GatewayTransactionId = null
);

public record RefundPaymentCommand(
    Guid OrderId,
    Guid PaymentId,
    decimal Amount,
    string Reason
);
