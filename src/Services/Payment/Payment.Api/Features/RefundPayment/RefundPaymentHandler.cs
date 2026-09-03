using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Payment.Api.Data;
using Payment.Api.Data.Extensions;
using Payment.Api.Domain.Errors;
using Payment.Api.Services;
using Payment.Contracts.Commands;
using Payment.Contracts.Events;
using Wolverine;

namespace Payment.Api.Features.RefundPayment;

public static class RefundPaymentHandler
{
    public static async Task<Result> Handle(
        RefundPaymentCommand command,
        PaymentDbContext dbContext,
        IStripePaymentGateway stripeGateway,
        IMessageBus bus,
        CancellationToken ct)
    {
        var query = dbContext.Payments.Include(p => p.Refunds);

        var payment = command.PaymentId.HasValue
            ? await query.SingleOrDefaultAsync(p => p.Id == command.PaymentId.Value, ct)
            : await query.WhereForOrder(command.OrderId).WhereSettled().OrderByLatest().FirstOrDefaultAsync(ct);

        if (payment is null)
        {
            return command.PaymentId.HasValue
                ? PaymentErrors.PaymentNotFound(command.PaymentId.Value)
                : PaymentErrors.PaymentNotFoundForOrder(command.OrderId);
        }

        var refundAmount = Price.Create(command.Amount);

        var canRefundResult = payment.CanRefund(refundAmount);
        if (canRefundResult.IsError)
        {
            return canRefundResult.Errors;
        }

        var refundId = Guid.CreateVersion7();

        var stripeRefundResult = await stripeGateway.RefundAsync(
            refundId,
            payment.PaymentIntentId!,
            command.Amount,
            command.Reason,
            ct
        );

        if (stripeRefundResult.IsError)
        {
            return stripeRefundResult.Errors;
        }

        var refundResult = payment.Refund(
            refundId,
            refundAmount,
            command.Reason,
            stripeRefundResult.Value
        );

        if (refundResult.IsError)
        {
            return refundResult.Errors;
        }

        await dbContext.SaveChangesAsync(ct);

        var refundedEvent = new PaymentRefundedEvent(
            payment.Id,
            payment.OrderId,
            stripeRefundResult.Value,
            command.Amount,
            payment.AmountRefunded,
            DateTime.UtcNow
        );

        await bus.PublishAsync(refundedEvent);

        return Result.Ok();
    }
}
