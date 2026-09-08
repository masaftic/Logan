using BuildingBlocks.Common.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Common.Extensions;

public static class ValidationExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseValidationExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ValidationExceptionMiddleware>();
    }
}
