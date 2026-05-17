using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace Ship24X7.Shared.Middleware;

/// <summary>
/// Middleware for managing correlation IDs across HTTP requests in the Ship24X7 platform.
/// Extracts correlation IDs from incoming requests or generates new ones, propagates them through
/// the request pipeline, adds them to response headers, and enriches logs for distributed tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes an HTTP request by extracting or generating a correlation ID,
    /// adding it to response headers, Serilog context, and HttpContext items for downstream access.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Workflow:
    /// 1. Extracts correlation ID from X-Correlation-Id request header if present
    /// 2. Generates a new GUID if no correlation ID is provided
    /// 3. Adds correlation ID to response headers for client tracking
    /// 4. Pushes correlation ID to Serilog context for automatic log enrichment
    /// 5. Stores correlation ID in HttpContext.Items for service layer access
    /// 6. Invokes the next middleware in the pipeline
    /// </remarks>
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Add to response headers
        context.Response.Headers[CorrelationIdHeader] = correlationId;
        
        // Add to Serilog context for logging
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            // Store in HttpContext for access by services
            context.Items["CorrelationId"] = correlationId;
            
            await _next(context);
        }
    }

    /// <summary>
    /// Extracts the correlation ID from the request header or generates a new one if not present.
    /// </summary>
    /// <param name="context">The HTTP context containing the request headers.</param>
    /// <returns>The correlation ID from the request header, or a newly generated GUID.</returns>
    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) 
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
