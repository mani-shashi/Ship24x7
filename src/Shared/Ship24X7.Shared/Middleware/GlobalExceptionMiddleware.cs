using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Ship24X7.Shared.Middleware;

/// <summary>
/// Global exception handling middleware for all Ship24X7 microservices.
///
/// Catches every unhandled exception that escapes the controller pipeline and maps it
/// to a consistent JSON error envelope:
///
///   { "error": "...", "correlationId": "...", "timestamp": "..." }
///
/// For FluentValidation failures the envelope also includes a "errors" field:
///   { "error": "Validation failed", "errors": { "field": ["msg"] }, ... }
///
/// Mapping rules:
///   ValidationException         → 400 Bad Request   (input validation failures)
///   InvalidOperationException   → 400 Bad Request   (domain rule violations)
///   UnauthorizedAccessException → 403 Forbidden
///   KeyNotFoundException        → 404 Not Found
///   ArgumentException           → 400 Bad Request
///   Everything else             → 500 Internal Server Error
///
/// Controllers should only handle the happy path and let domain exceptions bubble up.
/// The middleware logs every 5xx with full stack trace and the correlation ID so
/// distributed traces can be reconstructed from Serilog output.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString()
                         ?? context.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                         ?? Guid.NewGuid().ToString();

        context.Response.ContentType = "application/json";

        string body;

        if (exception is ValidationException validationEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

            var fieldErrors = validationEx.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            _logger.LogWarning(
                "Validation failed for {Path}. CorrelationId={CorrelationId}",
                context.Request.Path, correlationId);

            body = JsonSerializer.Serialize(new
            {
                error         = "Validation failed.",
                errors        = fieldErrors,
                correlationId = correlationId,
                timestamp     = DateTime.UtcNow
            });
        }
        else
        {
            var (statusCode, message) = exception switch
            {
                InvalidOperationException e  => (HttpStatusCode.BadRequest,         e.Message),
                UnauthorizedAccessException  => (HttpStatusCode.Forbidden,           "Access denied."),
                KeyNotFoundException e       => (HttpStatusCode.NotFound,            e.Message),
                ArgumentException e          => (HttpStatusCode.BadRequest,          e.Message),
                _                            => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.StatusCode = (int)statusCode;

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                using (LogContext.PushProperty("CorrelationId", correlationId))
                {
                    _logger.LogError(exception,
                        "Unhandled exception. CorrelationId={CorrelationId} Path={Path}",
                        correlationId, context.Request.Path);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Domain exception {ExceptionType}: {Message}. CorrelationId={CorrelationId}",
                    exception.GetType().Name, exception.Message, correlationId);
            }

            body = JsonSerializer.Serialize(new
            {
                error         = message,
                correlationId = correlationId,
                timestamp     = DateTime.UtcNow
            });
        }

        await context.Response.WriteAsync(body);
    }
}
