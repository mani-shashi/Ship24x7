using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Infrastructure.Persistence;
using Ship24X7.Auth.Infrastructure.Repositories;using Ship24X7.Auth.Infrastructure.Services;
using Ship24X7.Shared.Extensions;
using Ship24X7.Shared.Logging;
using Ship24X7.Shared.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = SerilogConfiguration.CreateLogger("AuthService");
builder.Host.UseSerilog();

// Add DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDb")));

// Add MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(Ship24X7.Auth.Application.Commands.LoginCommand).Assembly);
    cfg.AddBehavior(typeof(MediatR.IPipelineBehavior<,>), typeof(Ship24X7.Shared.Behaviours.ValidationBehaviour<,>));
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Ship24X7.Auth.Application.Commands.LoginCommand).Assembly);

// Add Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
builder.Services.AddScoped<IUserAddressRepository, UserAddressRepository>();
builder.Services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();

// Add Services
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();
builder.Services.AddScoped<IMfaService, TotpMfaService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddHttpClient<IOAuthService, GoogleOAuthService>();

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
    c.SwaggerDoc("v1", new() { Title = "Ship24X7 Auth API", Version = "v1" });
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
    .AddDbContextCheck<AuthDbContext>();

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
    options.ListenAnyIP(9001); // Auth service port
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
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await AuthDbSeeder.SeedAsync(dbContext);
}

Log.Information("Auth Service starting on port 9001");

app.Run();
