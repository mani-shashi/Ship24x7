using Microsoft.AspNetCore.Builder;
using Ship24X7.Shared.Middleware;

namespace Ship24X7.Shared.Extensions;

/// <summary>
/// Extension method to register the global exception middleware.
/// Must be placed BEFORE UseCorrelationId so the correlation ID is available
/// when the exception handler formats its response.
/// </summary>
public static class GlobalExceptionExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}
