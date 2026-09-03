using Payment.Api.Domain;
using Payment.Api.Domain.Enums;

namespace Payment.Api.Data.Extensions;

public static class PaymentQueryExtensions
{
    public static IQueryable<PaymentRecord> WhereForOrder(
        this IQueryable<PaymentRecord> query,
        Guid orderId)
        => query.Where(p => p.OrderId == orderId);

    public static IQueryable<PaymentRecord> WhereSettled(
        this IQueryable<PaymentRecord> query)
        => query.Where(p => p.Status == PaymentStatus.Succeeded || p.Status == PaymentStatus.PartiallyRefunded);

    public static IQueryable<PaymentRecord> WhereActiveOrSucceeded(
        this IQueryable<PaymentRecord> query)
        => query.Where(p => p.Status == PaymentStatus.Ready || p.Status == PaymentStatus.Succeeded);

    public static IQueryable<PaymentRecord> OrderByLatest(
        this IQueryable<PaymentRecord> query)
        => query.OrderByDescending(p => p.CreatedAtUtc);
}
