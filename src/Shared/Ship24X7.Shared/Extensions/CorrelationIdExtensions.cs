using Microsoft.AspNetCore.Builder;
using Ship24X7.Shared.Middleware;

namespace Ship24X7.Shared.Extensions;

/// <summary>
/// Provides extension methods for configuring correlation ID middleware in the ASP.NET Core pipeline.
/// Enables distributed tracing by propagating correlation IDs across HTTP requests and service boundaries.
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// Adds the correlation ID middleware to the application pipeline.
    /// This middleware extracts or generates correlation IDs for each request, adds them to response headers,
    /// and makes them available throughout the request lifecycle for logging and tracing.
    /// </summary>
    /// <param name="app">The application builder instance.</param>
    /// <returns>The application builder for method chaining.</returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
