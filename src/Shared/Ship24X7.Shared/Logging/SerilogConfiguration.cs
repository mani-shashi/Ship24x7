using Serilog;
using Serilog.Events;

namespace Ship24X7.Shared.Logging;

/// <summary>
/// Provides centralized Serilog configuration for all Ship24X7 microservices.
/// Configures structured logging with console and file sinks, enrichment with service context,
/// and consistent log formatting across the platform.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Creates a configured Serilog logger instance for a specific microservice.
    /// Configures logging to both console and rolling file with structured output including
    /// service name, correlation ID, timestamp, and log level.
    /// </summary>
    /// <param name="serviceName">The name of the microservice (e.g., "Auth", "Payment", "Shipment").</param>
    /// <returns>A configured Serilog logger instance ready for use.</returns>
    /// <remarks>
    /// Log Configuration:
    /// - Minimum level: Information (Warning for Microsoft framework logs)
    /// - Console output: Structured format with all enrichment properties
    /// - File output: Daily rolling logs in logs/{serviceName}-.log
    /// - File retention: 30 days
    /// - File size limit: 100 MB per file
    /// - Enrichment: ServiceName, MachineName, ThreadId, CorrelationId
    /// </remarks>
    public static ILogger CreateLogger(string serviceName)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

        var config = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.File(
                path: $"logs/{serviceName}-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{ServiceName}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}",
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 104857600); // 100 MB

        // Console logging only in Development — in Production, container runtime captures stdout from file
        if (isDevelopment)
        {
            config.WriteTo.Console(
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{ServiceName}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");
        }

        return config.CreateLogger();
    }
}
