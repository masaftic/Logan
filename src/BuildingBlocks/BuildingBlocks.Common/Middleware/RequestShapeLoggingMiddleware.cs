using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Common.Middleware;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public sealed class LogRequestShapeAttribute : Attribute
{
}

public class RequestShapeLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestShapeLoggingMiddleware> _logger;

    public RequestShapeLoggingMiddleware(RequestDelegate next, ILogger<RequestShapeLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await _next(context);
            return;
        }

        var shouldLog = endpoint.Metadata.GetMetadata<LogRequestShapeAttribute>() is not null
            || endpoint.Metadata.GetMetadata<ITagsMetadata>()?.Tags.Any(t =>
                string.Equals(t, "LogRequestShape", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(t, "LogRequest", StringComparison.OrdinalIgnoreCase)) is true;

        if (!shouldLog)
        {
            await _next(context);
            return;
        }

        context.Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(
            context.Request.Body,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            leaveOpen: true))
        {
            body = await reader.ReadToEndAsync(context.RequestAborted);
            context.Request.Body.Position = 0;
        }

        var headers = string.Join(
            Environment.NewLine,
            context.Request.Headers.Select(h => $"  {h.Key}: {h.Value}"));

        _logger.LogInformation(
            """
            [Request Shape] {Method} {Path}{Query}
            Headers:
            {Headers}
            Body:
            {Body}
            """,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString.Value,
            string.IsNullOrWhiteSpace(headers) ? "  (none)" : headers,
            string.IsNullOrWhiteSpace(body) ? "(empty)" : body);

        await _next(context);
    }
}
