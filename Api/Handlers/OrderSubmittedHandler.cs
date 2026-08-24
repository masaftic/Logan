using Contracts;
using Microsoft.Extensions.Logging;

namespace Api.Handlers;

public class OrderSubmittedHandler
{
    public static ProcessPayment Handle(OrderSubmitted @event, ILogger<OrderSubmittedHandler> logger)
    {
        logger.LogInformation("Order {OrderId} submitted for amount {Amount:C}. Initiating payment processing.",
            @event.OrderId, @event.Amount);

        // Cascading message: Wolverine automatically dispatches this returned command
        return new ProcessPayment(@event.OrderId, @event.Amount);
    }
}
