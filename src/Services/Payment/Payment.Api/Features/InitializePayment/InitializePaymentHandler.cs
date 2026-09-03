using BuildingBlocks.Common.Results;
using BuildingBlocks.Common.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Payment.Api.Data;
using Payment.Api.Data.Extensions;
using Payment.Api.Domain;
using Payment.Api.Domain.Enums;
using Payment.Api.Services;
using Payment.Contracts.Commands;
using Payment.Contracts.DTOs;
using Payment.Contracts.Enums;

namespace Payment.Api.Features.InitializePayment;

public static class InitializePaymentHandler
{
    public static async Task<Result<PaymentInitializationDto>> Handle(
        InitializePaymentCommand command,
        PaymentDbContext dbContext,
        IStripePaymentGateway stripeGateway,
        CancellationToken ct)
    {
        // Check for an active or successful existing payment for this order
        var existingActivePayment = await dbContext.Payments
            .WhereForOrder(command.OrderId)
            .WhereActiveOrSucceeded()
            .OrderByLatest()
            .FirstOrDefaultAsync(ct);

        if (existingActivePayment is not null)
        {
            var statusDto = existingActivePayment.Status == PaymentStatus.Succeeded
                ? PaymentStatusDto.Succeeded
                : PaymentStatusDto.Ready;

            return new PaymentInitializationDto(
                existingActivePayment.OrderId,
                statusDto,
                existingActivePayment.ClientSecret,
                existingActivePayment.PaymentIntentId,
                existingActivePayment.Amount,
                existingActivePayment.Currency
            );
        }

        // Create a new payment attempt
        var payment = PaymentRecord.CreatePending(
            command.OrderId,
            command.CustomerId,
            Price.Create(command.Amount),
            command.Currency
        );

        dbContext.Payments.Add(payment);

        var stripeResult = await stripeGateway.CreatePaymentIntentAsync(
            payment.Id,
            command.OrderId,
            command.Amount,
            command.Currency,
            ct
        );

        if (stripeResult.IsError)
        {
            var errorMessage = stripeResult.FirstError.Description;
            payment.MarkInitializationFailed(errorMessage);
            await dbContext.SaveChangesAsync(ct);

            return new PaymentInitializationDto(
                command.OrderId,
                PaymentStatusDto.InitializationFailed,
                ClientSecret: null,
                PaymentIntentId: null,
                Amount: command.Amount,
                Currency: command.Currency,
                ErrorMessage: errorMessage
            );
        }

        payment.MarkReady(stripeResult.Value.PaymentIntentId, stripeResult.Value.ClientSecret);
        await dbContext.SaveChangesAsync(ct);

        return new PaymentInitializationDto(
            command.OrderId,
            PaymentStatusDto.Ready,
            stripeResult.Value.ClientSecret,
            stripeResult.Value.PaymentIntentId,
            command.Amount,
            command.Currency
        );
    }
}
