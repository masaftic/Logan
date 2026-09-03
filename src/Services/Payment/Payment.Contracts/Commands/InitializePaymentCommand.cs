namespace Payment.Contracts.Commands;

public record InitializePaymentCommand(
    Guid OrderId,
    Guid CustomerId,
    decimal Amount,
    string Currency
);
