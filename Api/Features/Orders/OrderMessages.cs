using Humanizer;
using Wolverine;

namespace Api.Features.Orders;

public record OrderTimeout(Guid OrderId) : TimeoutMessage(30.Seconds());
public record RefundPayment(Guid OrderId, Guid PaymentId);
