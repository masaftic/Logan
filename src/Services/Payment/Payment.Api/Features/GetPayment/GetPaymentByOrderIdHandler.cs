using BuildingBlocks.Common.Results;
using Microsoft.EntityFrameworkCore;
using Payment.Api.Data;
using Payment.Api.Data.Extensions;
using Payment.Api.Domain.Errors;
using Payment.Contracts.DTOs;
using Payment.Contracts.Enums;
using Payment.Contracts.Queries;

namespace Payment.Api.Features.GetPayment;

public static class GetPaymentByOrderIdHandler
{
    public static async Task<Result<PaymentRecordDto>> Handle(
        GetPaymentByOrderIdQuery query,
        PaymentDbContext dbContext,
        CancellationToken ct)
    {
        var payment = await dbContext.Payments
            .Include(p => p.Refunds)
            .AsNoTracking()
            .WhereForOrder(query.OrderId)
            .OrderByLatest()
            .FirstOrDefaultAsync(ct);

        if (payment is null)
        {
            return PaymentErrors.PaymentNotFoundForOrder(query.OrderId);
        }

        var refundDtos = payment.Refunds
            .Select(r => new PaymentRefundDto(
                r.Id,
                r.Amount,
                r.Reason,
                r.ProviderRefundId,
                r.CreatedAtUtc))
            .ToList();

        return new PaymentRecordDto(
            payment.Id,
            payment.OrderId,
            payment.CustomerId,
            payment.Amount,
            payment.AmountRefunded,
            payment.RemainingAmount,
            payment.Currency,
            (PaymentStatusDto)payment.Status,
            payment.PaymentIntentId,
            payment.ClientSecret,
            refundDtos,
            payment.CreatedAtUtc,
            payment.CompletedAtUtc,
            payment.FailedAtUtc,
            payment.FailureReason
        );
    }
}
