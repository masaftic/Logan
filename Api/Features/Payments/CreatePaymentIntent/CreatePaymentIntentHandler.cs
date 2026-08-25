using Api.Data;
using Api.Features.Payments.Services;
using Contracts.Payments.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Api.Features.Payments.CreatePaymentIntent;

public static class CreatePaymentIntentHandler
{
    public static async Task<(PaymentIntentResponse, Contracts.Payments.Commands.ProcessPaymentCommand)> Handle(
        Contracts.Payments.Commands.CreatePaymentIntentCommand command,
        OrderDbContext dbContext,
        IPaymentGateway gateway,
        CancellationToken ct)
    {
        var order = await dbContext.Orders
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == command.OrderId, ct);

        if (order is null)
            throw new KeyNotFoundException($"Order with ID '{command.OrderId}' was not found.");

        if (order.Status == Api.Domain.Orders.OrderStatus.Completed)
            throw new InvalidOperationException("Order is already completed.");

        if (order.Status == Api.Domain.Orders.OrderStatus.Canceled || order.Status == Api.Domain.Orders.OrderStatus.TimedOut)
            throw new InvalidOperationException($"Cannot pay for an order in '{order.Status}' state.");

        // 1. Create PaymentIntent via Gateway
        var intent = await gateway.CreatePaymentIntentAsync(order.Id, order.Amount, ct);

        // 2. Record the payment attempt in the Order aggregate
        var idempotencyKey = $"pay_intent_{intent.PaymentIntentId}";
        var payment = order.AddPaymentAttempt(idempotencyKey, intent.PaymentIntentId);
        dbContext.Payments.Add(payment);

        var response = new PaymentIntentResponse(
            PaymentIntentId: intent.PaymentIntentId,
            ClientSecret: intent.ClientSecret,
            OrderId: order.Id,
            Amount: order.Amount,
            Status: payment.Status.ToString()
        );

        var processCmd = new Contracts.Payments.Commands.ProcessPaymentCommand(order.Id, payment.Id, order.Amount, idempotencyKey);

        return (response, processCmd);
    }
}
