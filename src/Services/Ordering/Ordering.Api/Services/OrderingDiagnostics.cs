using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Ordering.Api.Services;

public static class OrderingDiagnostics
{
    public const string ServiceName = "Logan.Ordering";

    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);

    public static readonly Counter<long> OrdersSubmittedCounter =
        Meter.CreateCounter<long>("orders.submitted.count", description: "Total number of orders submitted");

    public static readonly Histogram<decimal> OrderValueHistogram =
        Meter.CreateHistogram<decimal>("orders.total_amount", unit: "{currency}", description: "Value distribution of submitted orders");
}
