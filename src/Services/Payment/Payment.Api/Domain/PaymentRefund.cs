using BuildingBlocks.Common.ValueObjects;

namespace Payment.Api.Domain;

public class PaymentRefund
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public Price Amount { get; private set; }
    public string Reason { get; private set; } = null!;
    public string ProviderRefundId { get; private set; } = null!;
    public DateTime CreatedAtUtc { get; private set; }

    private PaymentRefund() { }

    public static PaymentRefund Create(
        Guid id,
        Guid paymentId,
        Price amount,
        string reason,
        string providerRefundId)
    {
        return new PaymentRefund
        {
            Id = id,
            PaymentId = paymentId,
            Amount = amount,
            Reason = reason,
            ProviderRefundId = providerRefundId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
