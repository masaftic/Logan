using BuildingBlocks.Common.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Common.Extensions;

public static class RequestShapeLoggingExtensions
{
    public static IApplicationBuilder UseRequestShapeLogging(this IApplicationBuilder app)
    {
        app.UseRouting();
        return app.UseMiddleware<RequestShapeLoggingMiddleware>();
    }

    public static TBuilder LogRequestShape<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
    {
        return builder.WithMetadata(new LogRequestShapeAttribute());
    }
}
