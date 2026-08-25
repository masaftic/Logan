namespace Contracts.Orders.Commands;

public record CreateOrderCommand(decimal Amount);
public record CancelOrderCommand(Guid OrderId);
