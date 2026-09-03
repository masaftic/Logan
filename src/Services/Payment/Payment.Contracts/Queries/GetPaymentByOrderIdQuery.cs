namespace Payment.Contracts.Queries;

public record GetPaymentByOrderIdQuery(
    Guid OrderId
);
