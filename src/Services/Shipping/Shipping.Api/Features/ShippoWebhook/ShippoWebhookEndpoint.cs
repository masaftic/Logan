using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Common.Extensions;
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

            if (!string.Equals(payload.Event, "track_updated", StringComparison.OrdinalIgnoreCase))
            {
                return Results.Ok();
            }

            var trackingInfo = payload.ExtractTrackingInfo();
            if (trackingInfo is null)
            {
                return Results.Ok();
            }

            var command = new CommandProcessShippoWebhook(
                TrackingNumber: trackingInfo.Value.TrackingNumber,
                Status: trackingInfo.Value.Status,
                Message: trackingInfo.Value.Message,
                Location: trackingInfo.Value.Location,
                OccurredAtUtc: trackingInfo.Value.OccurredAtUtc);

            await bus.InvokeAsync(command, ct);

            return Results.Ok();
        })
        .WithName("ShippoWebhook")
        .WithSummary("Handle incoming Shippo tracking webhooks with token verification")
        .WithTags("Shipping")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .LogRequestShape();
    }
}

public record ShippoWebhookPayload(
    string? Event,
    JsonElement? Data,
    [property: JsonPropertyName("tracking_number")] string? TrackingNumber,
    [property: JsonPropertyName("tracking_status")] JsonElement? TrackingStatus)
{
    public (string TrackingNumber, string Status, string Message, string? Location, DateTime OccurredAtUtc)? ExtractTrackingInfo()
    {
        string? trackingNumber = TrackingNumber;
        string? status = null;
        string? statusDetails = null;
        DateTime? statusDate = null;
        string? location = null;

        if (Data.HasValue && Data.Value.ValueKind == JsonValueKind.Object)
        {
            if (Data.Value.TryGetProperty("tracking_number", out var numProp) && numProp.ValueKind == JsonValueKind.String)
            {
                trackingNumber ??= numProp.GetString();
            }

            if (Data.Value.TryGetProperty("tracking_status", out var statusProp))
            {
                ParseStatus(statusProp, ref status, ref statusDetails, ref statusDate, ref location);
            }
        }

        if (TrackingStatus.HasValue)
        {
            ParseStatus(TrackingStatus.Value, ref status, ref statusDetails, ref statusDate, ref location);
        }

        if (string.IsNullOrWhiteSpace(trackingNumber) || string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return (
            trackingNumber,
            status,
            statusDetails ?? status,
            location,
            statusDate ?? DateTime.UtcNow
        );
    }

    private static void ParseStatus(
        JsonElement element,
        ref string? status,
        ref string? statusDetails,
        ref DateTime? statusDate,
        ref string? location)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            status ??= element.GetString();
            statusDetails ??= status;
        }
        else if (element.ValueKind == JsonValueKind.Object)
        {
            var parsed = element.Deserialize<ShippoTrackingStatus>();
            if (parsed is not null)
            {
                status ??= parsed.Status;
                statusDetails ??= parsed.StatusDetails ?? parsed.Status;
                statusDate ??= parsed.StatusDate;
                location ??= parsed.Location?.Format();
            }
        }
    }
}

public record ShippoTrackingStatus(
    [property: JsonPropertyName("status")] string? Status,
    [property: JsonPropertyName("status_details")] string? StatusDetails,
    [property: JsonPropertyName("status_date")] DateTime? StatusDate,
    [property: JsonPropertyName("location")] ShippoTrackingLocation? Location);

public record ShippoTrackingLocation(
    [property: JsonPropertyName("city")] string? City,
    [property: JsonPropertyName("state")] string? State,
    [property: JsonPropertyName("zip")] string? Zip,
    [property: JsonPropertyName("country")] string? Country)
{
    public string? Format()
    {
        var parts = new[] { City, State, Zip, Country }.Where(s => !string.IsNullOrWhiteSpace(s));
        var result = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }
}
