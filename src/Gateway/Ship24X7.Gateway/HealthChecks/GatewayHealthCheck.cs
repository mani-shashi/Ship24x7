using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Ship24X7.Gateway.HealthChecks;

/// <summary>
/// Implements health check functionality for the API Gateway service.
/// Provides basic health status monitoring to ensure the gateway is operational.
/// </summary>
public class GatewayHealthCheck : IHealthCheck
{
    private readonly ILogger<GatewayHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GatewayHealthCheck"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for recording health check operations and errors.</param>
    public GatewayHealthCheck(ILogger<GatewayHealthCheck> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Performs a health check on the API Gateway service.
    /// Returns healthy status if the gateway is running, unhealthy if any errors occur.
    /// </summary>
    /// <param name="context">Context information for the health check execution.</param>
    /// <param name="cancellationToken">Token to cancel the health check operation.</param>
    /// <returns>A task containing the health check result with status, timestamp, and service information.</returns>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic health check - gateway is running
            var data = new Dictionary<string, object>
            {
                { "status", "healthy" },
                { "timestamp", DateTime.UtcNow },
                { "service", "API Gateway" }
            };

            return Task.FromResult(
                HealthCheckResult.Healthy("API Gateway is running", data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(
                HealthCheckResult.Unhealthy("API Gateway health check failed", ex));
        }
    }
}
