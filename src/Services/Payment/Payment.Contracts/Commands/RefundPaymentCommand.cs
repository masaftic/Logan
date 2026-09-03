namespace Payment.Contracts.Commands;

public record RefundPaymentCommand(
    Guid OrderId,
    decimal Amount,
    string Reason,
    Guid? PaymentId = null
);
