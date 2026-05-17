using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using Ship24X7.Gateway.HealthChecks;
using Xunit;

namespace Ship24X7.Gateway.Tests;

/// <summary>
/// Contains unit tests for the Gateway health check functionality.
/// Validates that the health check correctly reports the gateway's operational status.
/// </summary>
public class HealthCheckTests
{
    /// <summary>
    /// Verifies that the Gateway health check returns a healthy status when the gateway is operational.
    /// Ensures the health check includes required metadata like status, timestamp, and service name.
    /// </summary>
    [Fact]
    public async Task GatewayHealthCheck_ShouldReturnHealthy()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GatewayHealthCheck>>();
        var healthCheck = new GatewayHealthCheck(mockLogger.Object);
        var context = new HealthCheckContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("API Gateway is running");
        result.Data.Should().ContainKey("status");
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("service");
    }

    /// <summary>
    /// Verifies that the Gateway health check includes a timestamp in its response data.
    /// The timestamp should be accurate and fall within the time window of the health check execution.
    /// </summary>
    [Fact]
    public async Task GatewayHealthCheck_ShouldIncludeTimestamp()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<GatewayHealthCheck>>();
        var healthCheck = new GatewayHealthCheck(mockLogger.Object);
        var context = new HealthCheckContext();
        var beforeCheck = DateTime.UtcNow;

        // Act
        var result = await healthCheck.CheckHealthAsync(context);
        var afterCheck = DateTime.UtcNow;

        // Assert
        result.Data.Should().ContainKey("timestamp");
        var timestamp = (DateTime)result.Data["timestamp"];
        timestamp.Should().BeOnOrAfter(beforeCheck);
        timestamp.Should().BeOnOrBefore(afterCheck);
    }
}
