using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Prometheus;
using Serilog;
using Ship24X7.Shared.Extensions;
using Ship24X7.Shared.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = SerilogConfiguration.CreateLogger("APIGateway");
builder.Host.UseSerilog();

// Add Ocelot configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:SecretKey"] 
    ?? throw new InvalidOperationException("Jwt:SecretKey configuration is not set");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Ship24X7";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Ship24X7API";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Debug("JWT token validated for user: {UserId}", 
                    context.Principal?.FindFirst("sub")?.Value ?? "Unknown");
                return Task.CompletedTask;
            }
        };
    });

// Add Ocelot with Polly for QoS (circuit breaker and retry)
builder.Services.AddOcelot()
    .AddPolly();

// Add health checks
builder.Services.AddHealthChecks()
    .AddCheck<Ship24X7.Gateway.HealthChecks.GatewayHealthCheck>("gateway");

// Configure CORS with environment variable
var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
    ?? "http://localhost:4200";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Kestrel to listen on specific ports
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8000); // HTTP
    options.ListenAnyIP(9000, listenOptions =>
    {
        // HTTPS configuration - in production, load certificate from environment
        // listenOptions.UseHttps("certificate.pfx", "password");
    });
});

var app = builder.Build();

// Configure graceful shutdown
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Log.Information("API Gateway is shutting down gracefully...");
});

// Serve static files (for landing page)
app.UseDefaultFiles();
app.UseStaticFiles();

// HTTPS redirection
app.UseHttpsRedirection();

// Correlation ID middleware (must be first)
app.UseCorrelationId();

// Serilog request logging
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        diagnosticContext.Set("CorrelationId", httpContext.Items["CorrelationId"]?.ToString());
    };
});

// CORS
app.UseCors("AllowFrontend");

// Authentication
app.UseAuthentication();

// Prometheus metrics endpoint
app.UseMetricServer("/metrics");
app.UseHttpMetrics();

// Health check endpoint — short-circuit BEFORE Ocelot middleware.
// Ocelot intercepts all requests including /health, so we handle it
// explicitly here using a terminal middleware branch.
app.UseHealthChecks("/health");

Log.Information("API Gateway starting on HTTP port 8000 and HTTPS port 9000");
Log.Information("CORS allowed origins: {Origins}", allowedOrigins);
Log.Information("JWT Issuer: {Issuer}, Audience: {Audience}", jwtIssuer, jwtAudience);

// Ocelot middleware (must be last — intercepts all other routes)
await app.UseOcelot();

app.Run();
