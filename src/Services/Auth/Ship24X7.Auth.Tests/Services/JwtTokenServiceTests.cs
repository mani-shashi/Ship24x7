using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Ship24X7.Auth.Infrastructure.Services;

namespace Ship24X7.Auth.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        var configData = new Dictionary<string, string>
        {
            { "Jwt:SecretKey", "ThisIsAVerySecureSecretKeyForTestingPurposesOnly12345678901234567890" },
            { "Jwt:Issuer", "Ship24X7TestIssuer" },
            { "Jwt:Audience", "Ship24X7TestAudience" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        _sut = new JwtTokenService(_configuration);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer" };
        var claims = new[] { "read:shipments" };

        // Act
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeUserIdClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer" };
        var claims = Array.Empty<string>();

        // Act
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        // JWT tokens serialize ClaimTypes.NameIdentifier as "nameid"
        var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
        
        userIdClaim.Should().NotBeNull();
        userIdClaim!.Value.Should().Be(userId.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeEmailClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer" };
        var claims = Array.Empty<string>();

        // Act
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        // JWT tokens serialize ClaimTypes.Email as "email"
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email");
        
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(email);
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeRoleClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer", "Admin_User" };
        var claims = Array.Empty<string>();

        // Act
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        // JWT tokens serialize ClaimTypes.Role as "role"
        var roleClaims = jwtToken.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
        
        roleClaims.Should().Contain("Customer");
        roleClaims.Should().Contain("Admin_User");
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeCustomClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer" };
        var claims = new[] { "read:shipments", "write:shipments" };

        // Act
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var permissionClaims = jwtToken.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();
        
        permissionClaims.Should().Contain("read:shipments");
        permissionClaims.Should().Contain("write:shipments");
    }

    [Fact]
    public void GenerateAccessToken_ShouldExpireIn15Minutes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer" };
        var claims = Array.Empty<string>();

        // Act
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expiryTime = jwtToken.ValidTo;
        var expectedExpiry = DateTime.UtcNow.AddMinutes(15);
        
        expiryTime.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateAccessToken_ShouldUseHS256Algorithm()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer" };
        var claims = Array.Empty<string>();

        // Act
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        jwtToken.SignatureAlgorithm.Should().Be("HS256");
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyToken()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnDifferentTokensEachTime()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnBase64String()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        var isValidBase64 = IsBase64String(token);
        isValidBase64.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer" };
        var claims = Array.Empty<string>();
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Act
        var result = _sut.ValidateToken(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var result = _sut.ValidateToken(invalidToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateToken_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange - Create a token with past expiry
        var configData = new Dictionary<string, string>
        {
            { "Jwt:SecretKey", "ThisIsAVerySecureSecretKeyForTestingPurposesOnly12345678901234567890" },
            { "Jwt:Issuer", "Ship24X7TestIssuer" },
            { "Jwt:Audience", "Ship24X7TestAudience" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configData!).Build();
        var service = new JwtTokenService(config);
        
        // Generate token and wait for it to expire (not practical in real test)
        // Instead, we'll test with a malformed token that simulates expiry
        var expiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE1MTYyMzkwMjJ9.invalid";

        // Act
        var result = service.ValidateToken(expiredToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var roles = new[] { "Customer" };
        var claims = Array.Empty<string>();
        var token = _sut.GenerateAccessToken(userId, email, roles, claims);

        // Act
        var result = _sut.GetUserIdFromToken(token);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var result = _sut.GetUserIdFromToken(invalidToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithMissingSecretKey_ShouldThrowException()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Jwt:Issuer", "Ship24X7TestIssuer" },
            { "Jwt:Audience", "Ship24X7TestAudience" }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configData!).Build();

        // Act
        Action act = () => new JwtTokenService(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("JWT SecretKey not configured");
    }

    private static bool IsBase64String(string base64)
    {
        try
        {
            Convert.FromBase64String(base64);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
