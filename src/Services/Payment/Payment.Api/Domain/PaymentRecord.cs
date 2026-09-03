using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Payment.Api.Domain.Enums;
using Payment.Api.Domain.Errors;

namespace Payment.Api.Domain;

public class PaymentRecord
{
    private readonly List<PaymentRefund> _refunds = [];

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Price Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public PaymentStatus Status { get; private set; }
    public string? PaymentIntentId { get; private set; }
    public string? ClientSecret { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? FailedAtUtc { get; private set; }

    public IReadOnlyCollection<PaymentRefund> Refunds => _refunds.AsReadOnly();

    public Price AmountRefunded => (Price)_refunds.Sum(r => r.Amount);
    public Price RemainingAmount => Amount - AmountRefunded;

    private PaymentRecord() { }

    public static PaymentRecord CreatePending(
        Guid orderId,
        Guid customerId,
        Price amount,
        string currency)
    {
        return new PaymentRecord
        {
            Id = Guid.CreateVersion7(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Currency = currency.Trim().ToLowerInvariant(),
            Status = PaymentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void MarkReady(string paymentIntentId, string clientSecret)
    {
        PaymentIntentId = paymentIntentId;
        ClientSecret = clientSecret;
        Status = PaymentStatus.Ready;
    }

    public void MarkInitializationFailed(string reason)
    {
        Status = PaymentStatus.InitializationFailed;
        FailureReason = reason;
    }

    public void MarkSucceeded(DateTime completedAtUtc)
    {
        if (Status == PaymentStatus.Succeeded)
        {
            return;
        }

        Status = PaymentStatus.Succeeded;
        CompletedAtUtc = completedAtUtc;
    }

    public void MarkFailed(string reason)
    {
        if (Status == PaymentStatus.Failed)
        {
            return;
        }

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        FailedAtUtc = DateTime.UtcNow;
    }

    public Result<PaymentRefund> Refund(
        Guid refundId,
        Price amount,
        string reason,
        string providerRefundId)
    {
        var canRefundResult = CanRefund(amount);
        if (canRefundResult.IsError)
            return canRefundResult.Errors;

        var refund = PaymentRefund.Create(refundId, Id, amount, reason, providerRefundId);
        _refunds.Add(refund);

        if (RemainingAmount <= 0)
        {
            Status = PaymentStatus.Refunded;
        }
        else
        {
            Status = PaymentStatus.PartiallyRefunded;
        }

        return refund;
    }

    public Result CanRefund(Price amount)
    {
        if (Status != PaymentStatus.Succeeded && Status != PaymentStatus.PartiallyRefunded)
        {
            return PaymentErrors.CannotRefundUnsettledPayment(Id, Status.ToString());
        }

        if (amount > RemainingAmount)
        {
            return PaymentErrors.RefundExceedsRemainingBalance(Id, amount, RemainingAmount);
        }

        return Result.Ok();
    }
}
