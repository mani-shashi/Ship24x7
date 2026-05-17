using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Ship24X7.Gateway.Tests;

/// <summary>
/// Contains unit tests for validating the Ocelot API Gateway configuration.
/// Tests verify routing, rate limiting, circuit breaker, and security settings.
/// </summary>
public class GatewayConfigurationTests
{
    /// <summary>
    /// Loads the Ocelot configuration from the ocelot.json file.
    /// </summary>
    /// <returns>An IConfiguration instance containing the Ocelot configuration settings.</returns>
    private static IConfiguration LoadOcelotConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("ocelot.json", optional: false)
            .Build();
    }

    /// <summary>
    /// Verifies that the Ocelot configuration contains all required routes for the microservices.
    /// Ensures at least 5 routes are configured (one for each microservice).
    /// </summary>
    [Fact]
    public void OcelotConfiguration_ShouldHaveAllRequiredRoutes()
    {
        // Arrange
        var configuration = LoadOcelotConfiguration();

        // Act
        var routes = configuration.GetSection("Routes").GetChildren().ToList();

        // Assert
        routes.Should().NotBeEmpty();
        routes.Count.Should().BeGreaterOrEqualTo(5, "because we have 5 microservices");
    }

    /// <summary>
    /// Verifies that rate limiting is enabled in the Ocelot global configuration.
    /// Checks that HTTP status code 429 (Too Many Requests) is configured for rate limit violations.
    /// </summary>
    [Fact]
    public void OcelotConfiguration_ShouldHaveRateLimitingEnabled()
    {
        // Arrange
        var configuration = LoadOcelotConfiguration();

        // Act
        var globalRateLimitOptions = configuration.GetSection("GlobalConfiguration:RateLimitOptions");

        // Assert
        globalRateLimitOptions.Should().NotBeNull();
        globalRateLimitOptions["HttpStatusCode"].Should().Be("429");
    }

    /// <summary>
    /// Verifies that circuit breaker (QoS) is configured for routes.
    /// Checks that routes have proper exception threshold and break duration settings.
    /// </summary>
    [Fact]
    public void OcelotConfiguration_ShouldHaveCircuitBreakerConfigured()
    {
        // Arrange
        var configuration = LoadOcelotConfiguration();

        // Act
        var routes = configuration.GetSection("Routes").GetChildren().ToList();
        var firstRouteQoS = routes.First().GetSection("QoSOptions");

        // Assert
        firstRouteQoS.Should().NotBeNull();
        firstRouteQoS["ExceptionsAllowedBeforeBreaking"].Should().Be("5");
        firstRouteQoS["DurationOfBreak"].Should().Be("30000");
    }

    /// <summary>
    /// Verifies that authenticated routes have a higher rate limit (100 requests per minute).
    /// Authenticated users should have more generous rate limits than anonymous users.
    /// </summary>
    [Fact]
    public void OcelotConfiguration_AuthenticatedRoutes_ShouldHaveHigherRateLimit()
    {
        // Arrange
        var configuration = LoadOcelotConfiguration();

        // Act
        var routes = configuration.GetSection("Routes").GetChildren().ToList();
        var authenticatedRoute = routes.FirstOrDefault(r => 
            r.GetSection("AuthenticationOptions")["AuthenticationProviderKey"] == "Bearer");

        // Assert
        authenticatedRoute.Should().NotBeNull();
        authenticatedRoute!.GetSection("RateLimitOptions")["Limit"].Should().Be("100");
    }

    /// <summary>
    /// Verifies that public routes have a lower rate limit (20 requests per minute).
    /// Public routes like authentication endpoints should have stricter rate limits to prevent abuse.
    /// </summary>
    [Fact]
    public void OcelotConfiguration_PublicRoutes_ShouldHaveLowerRateLimit()
    {
        // Arrange
        var configuration = LoadOcelotConfiguration();

        // Act
        var routes = configuration.GetSection("Routes").GetChildren().ToList();
        var publicRoute = routes.FirstOrDefault(r => 
            r["UpstreamPathTemplate"]?.Contains("/auth/") == true);

        // Assert
        publicRoute.Should().NotBeNull();
        publicRoute!.GetSection("RateLimitOptions")["Limit"].Should().Be("20");
    }

    /// <summary>
    /// Verifies that all routes include the X-Correlation-Id header for request tracing.
    /// Correlation IDs enable distributed tracing across microservices.
    /// </summary>
    [Fact]
    public void OcelotConfiguration_AllRoutes_ShouldHaveCorrelationIdHeader()
    {
        // Arrange
        var configuration = LoadOcelotConfiguration();

        // Act
        var routes = configuration.GetSection("Routes").GetChildren().ToList();

        // Assert
        foreach (var route in routes)
        {
            var headers = route.GetSection("AddHeadersToRequest");
            headers.Should().NotBeNull();
            headers["X-Correlation-Id"].Should().NotBeNullOrEmpty();
        }
    }

    /// <summary>
    /// Verifies that each service is routed to the correct downstream port.
    /// Tests routing configuration for auth, shipments, tracking, notifications, and payments services.
    /// </summary>
    /// <param name="service">The service name to test (e.g., "auth", "shipments").</param>
    /// <param name="expectedPort">The expected downstream port number for the service.</param>
    [Theory]
    [InlineData("auth", 9001)]
    [InlineData("shipments", 9002)]
    [InlineData("tracking", 9003)]
    [InlineData("notifications", 9004)]
    [InlineData("payments", 9005)]
    public void OcelotConfiguration_ShouldRouteToCorrectDownstreamPort(string service, int expectedPort)
    {
        // Arrange
        var configuration = LoadOcelotConfiguration();

        // Act
        var routes = configuration.GetSection("Routes").GetChildren().ToList();
        var serviceRoute = routes.FirstOrDefault(r => 
            r["UpstreamPathTemplate"]?.Contains($"/{service}/") == true);

        // Assert
        serviceRoute.Should().NotBeNull();
        var downstreamPort = serviceRoute!.GetSection("DownstreamHostAndPorts")
            .GetChildren()
            .First()["Port"];
        downstreamPort.Should().Be(expectedPort.ToString());
    }
}
