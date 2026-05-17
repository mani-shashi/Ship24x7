using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Ship24X7.Tracking.Application.Interfaces;
using Ship24X7.Tracking.Infrastructure.Messaging;
using Ship24X7.Tracking.Infrastructure.Persistence;
using Ship24X7.Tracking.Infrastructure.Repositories;
using Ship24X7.Tracking.Infrastructure.Services;
using Ship24X7.Shared.Extensions;
using Ship24X7.Shared.Logging;
using Ship24X7.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = SerilogConfiguration.CreateLogger("TrackingService");
builder.Host.UseSerilog();

// Add DbContext
builder.Services.AddDbContext<TrackingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TrackingDb")));

// Add MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Ship24X7.Tracking.Application.Commands.RecordTrackingEventCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Ship24X7.Tracking.Infrastructure.EventHandlers.ShipmentDeliveredEventHandler).Assembly);
    cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(Ship24X7.Shared.Behaviours.ValidationBehaviour<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Ship24X7.Tracking.Application.Commands.RecordTrackingEventCommand).Assembly);

// Add Repositories
builder.Services.AddScoped<ITrackingEventRepository, TrackingEventRepository>();
builder.Services.AddScoped<IDeliveryProofRepository, DeliveryProofRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Add Services
var storageBasePath = builder.Configuration["Storage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "storage");
builder.Services.AddSingleton<IDocumentStorageService>(sp => new BlobStorageService(storageBasePath));
builder.Services.AddScoped<ILabelGenerationService, LabelGenerationService>();
builder.Services.AddScoped<ICustomsFormService, CustomsFormService>();

// Add RabbitMQ Event Publisher
builder.Services.AddSingleton<TrackingEventPublisher>();

// Add RabbitMQ Consumer for Shipment Events — seeds the first tracking event
// automatically when a shipment is booked or payment is captured.
builder.Services.AddHostedService<ShipmentEventConsumer>();

// Add JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
var key = Encoding.UTF8.GetBytes(jwtSecretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "Ship24X7",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "Ship24X7",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Ship24X7 Tracking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<TrackingDbContext>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration["Cors:AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:4200" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Kestrel to listen on specific port
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(9003); // Tracking service port
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseGlobalExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Seed database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
    await TrackingDbSeeder.SeedAsync(dbContext);
}

Log.Information("Tracking Service starting on port 9003");

app.Run();
