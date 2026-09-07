using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Shipping.Api.Services;
using Shipping.Contracts.Commands;
using Wolverine;

namespace Shipping.Api.Features.ShippoWebhook;

public static class ShippoWebhookEndpoint
{
    public static void MapShippoWebhookEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/shipping/webhooks/shippo", async (
            [FromQuery] string? token,
            [FromBody] ShippoWebhookPayload payload,
            IOptions<ShippoOptions> options,
            IMessageBus bus,
            CancellationToken ct) =>
        {
            if (!string.IsNullOrEmpty(options.Value.WebhookSecret) && token != options.Value.WebhookSecret)
            {
                return Results.Unauthorized();
            }

            var trackingNumber = payload.Data?.TrackingNumber ?? payload.TrackingNumber;
            var trackingStatus = payload.Data?.TrackingStatus ?? payload.TrackingStatus;

            if (string.IsNullOrWhiteSpace(trackingNumber) || trackingStatus?.Status is null)
            {
                return Results.Ok();
            }

            var command = new CommandProcessShippoWebhook(
                TrackingNumber: trackingNumber,
                Status: trackingStatus.Status,
                Message: trackingStatus.StatusDetails ?? trackingStatus.Status,
                Location: trackingStatus.Location?.Format(),
                OccurredAtUtc: trackingStatus.StatusDate ?? DateTime.UtcNow);

            await bus.InvokeAsync(command, ct);

            return Results.Ok();
        })
        .WithName("ShippoWebhook")
        .WithSummary("Handle incoming Shippo tracking webhooks with token verification")
        .WithTags("Shipping")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized);
    }
}

public record ShippoWebhookPayload(
    string? Event,
    ShippoTrackingData? Data,
    [property: JsonPropertyName("tracking_number")] string? TrackingNumber,
    [property: JsonPropertyName("tracking_status")] ShippoTrackingStatus? TrackingStatus);

public record ShippoTrackingData(
    [property: JsonPropertyName("tracking_number")] string? TrackingNumber,
    [property: JsonPropertyName("tracking_status")] ShippoTrackingStatus? TrackingStatus);

public record ShippoTrackingStatus(
    string? Status,
    [property: JsonPropertyName("status_details")] string? StatusDetails,
    [property: JsonPropertyName("status_date")] DateTime? StatusDate,
    ShippoTrackingLocation? Location);

public record ShippoTrackingLocation(
    string? City,
    string? State,
    string? Zip,
    string? Country)
{
    public string? Format()
    {
        var parts = new[] { City, State, Zip, Country }.Where(s => !string.IsNullOrWhiteSpace(s));
        var result = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }
}
