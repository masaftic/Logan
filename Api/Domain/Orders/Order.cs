using Api.Domain.Payments;

namespace Api.Domain.Orders;

public class Order
{
    public Guid Id { get; private set; }
    public decimal Amount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private readonly List<Payment> _payments = new();
    public IReadOnlyCollection<Payment> Payments => _payments.AsReadOnly();

    // Required by EF Core
    private Order() { }

    public static Order Create(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Order amount must be greater than zero.", nameof(amount));

        return new Order
        {
            Id = Guid.NewGuid(),
            Amount = amount,
            Status = OrderStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void StartProcessing()
    {
        if (Status != OrderStatus.Pending)
            return;

        Status = OrderStatus.Processing;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Complete(Guid paymentId)
    {
        if (Status == OrderStatus.Completed)
            return; // Idempotent

        if (Status == OrderStatus.Canceled || Status == OrderStatus.TimedOut)
            throw new InvalidOperationException($"Cannot complete order in '{Status}' state.");

        Status = OrderStatus.Completed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot fail an already completed order.");

        Status = OrderStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Timeout()
    {
        if (Status == OrderStatus.Completed)
            return; // Ignore late timeouts

        Status = OrderStatus.TimedOut;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel an already completed order.");

        Status = OrderStatus.Canceled;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Payment AddPaymentAttempt(string idempotencyKey, string? gatewayTransactionId = null)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Canceled || Status == OrderStatus.TimedOut)
            throw new InvalidOperationException($"Cannot create payment attempt for order in '{Status}' state.");

        var payment = Payment.Create(Id, Amount, idempotencyKey, gatewayTransactionId);
        _payments.Add(payment);
        return payment;
    }
}
