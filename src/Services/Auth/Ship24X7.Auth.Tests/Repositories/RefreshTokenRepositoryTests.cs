using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Infrastructure.Persistence;
using Ship24X7.Auth.Infrastructure.Repositories;

namespace Ship24X7.Auth.Tests.Repositories;

public class RefreshTokenRepositoryTests : IDisposable
{
    private readonly AuthDbContext _context;
    private readonly RefreshTokenRepository _sut;

    public RefreshTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthDbContext(options);
        _sut = new RefreshTokenRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddRefreshTokenToDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "test-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            TokenFamily = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        // Act
        var result = await _sut.AddAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(refreshToken.Id);
        
        var savedToken = await _context.RefreshTokens.FindAsync(refreshToken.Id);
        savedToken.Should().NotBeNull();
        savedToken!.Token.Should().Be("test-refresh-token");
    }

    [Fact]
    public async Task GetByTokenAsync_WithExistingToken_ShouldReturnToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        await _context.Users.AddAsync(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "existing-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            TokenFamily = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByTokenAsync("existing-token");

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("existing-token");
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetByTokenAsync_WithNonExistingToken_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetByTokenAsync("non-existing-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateRefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        await _context.Users.AddAsync(user);

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "token-to-update",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            TokenFamily = Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _sut.UpdateAsync(refreshToken);

        // Assert
        var updatedToken = await _context.RefreshTokens.FindAsync(refreshToken.Id);
        updatedToken.Should().NotBeNull();
        updatedToken!.IsRevoked.Should().BeTrue();
        updatedToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeAllForUserAsync_ShouldRevokeAllUserTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        await _context.Users.AddAsync(user);

        var token1 = CreateRefreshToken(userId, "token1");
        var token2 = CreateRefreshToken(userId, "token2");
        var token3 = CreateRefreshToken(userId, "token3");
        
        await _context.RefreshTokens.AddRangeAsync(token1, token2, token3);
        await _context.SaveChangesAsync();

        // Act
        await _sut.RevokeAllForUserAsync(userId);

        // Assert
        var tokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        tokens.Should().AllSatisfy(t =>
        {
            t.IsRevoked.Should().BeTrue();
            t.RevokedAt.Should().NotBeNull();
        });
    }

    [Fact]
    public async Task RevokeAllForUserAsync_ShouldNotAffectOtherUsers()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        
        var user1 = CreateTestUser(userId1);
        var user2 = CreateTestUser(userId2);
        await _context.Users.AddRangeAsync(user1, user2);

        var token1 = CreateRefreshToken(userId1, "token1");
        var token2 = CreateRefreshToken(userId2, "token2");
        
        await _context.RefreshTokens.AddRangeAsync(token1, token2);
        await _context.SaveChangesAsync();

        // Act
        await _sut.RevokeAllForUserAsync(userId1);

        // Assert
        var user1Tokens = await _context.RefreshTokens.Where(t => t.UserId == userId1).ToListAsync();
        var user2Tokens = await _context.RefreshTokens.Where(t => t.UserId == userId2).ToListAsync();
        
        user1Tokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
        user2Tokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeFalse());
    }

    [Fact]
    public async Task RevokeTokenFamilyAsync_ShouldRevokeAllTokensInFamily()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        await _context.Users.AddAsync(user);

        var tokenFamily = Guid.NewGuid().ToString("N");
        var token1 = CreateRefreshToken(userId, "token1", tokenFamily);
        var token2 = CreateRefreshToken(userId, "token2", tokenFamily);
        var token3 = CreateRefreshToken(userId, "token3", Guid.NewGuid().ToString("N")); // Different family
        
        await _context.RefreshTokens.AddRangeAsync(token1, token2, token3);
        await _context.SaveChangesAsync();

        // Act
        await _sut.RevokeTokenFamilyAsync(tokenFamily);

        // Assert
        var familyTokens = await _context.RefreshTokens
            .Where(t => t.TokenFamily == tokenFamily)
            .ToListAsync();
        var otherTokens = await _context.RefreshTokens
            .Where(t => t.TokenFamily != tokenFamily)
            .ToListAsync();
        
        familyTokens.Should().AllSatisfy(t =>
        {
            t.IsRevoked.Should().BeTrue();
            t.RevokedAt.Should().NotBeNull();
        });
        otherTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeFalse());
    }

    [Fact]
    public async Task RevokeTokenFamilyAsync_ShouldNotRevokeAlreadyRevokedTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        await _context.Users.AddAsync(user);

        var tokenFamily = Guid.NewGuid().ToString("N");
        var revokedTime = DateTime.UtcNow.AddHours(-1);
        
        var token1 = CreateRefreshToken(userId, "token1", tokenFamily);
        token1.IsRevoked = true;
        token1.RevokedAt = revokedTime;
        
        var token2 = CreateRefreshToken(userId, "token2", tokenFamily);
        
        await _context.RefreshTokens.AddRangeAsync(token1, token2);
        await _context.SaveChangesAsync();

        // Act
        await _sut.RevokeTokenFamilyAsync(tokenFamily);

        // Assert
        var token1Updated = await _context.RefreshTokens.FindAsync(token1.Id);
        token1Updated!.RevokedAt.Should().Be(revokedTime, "already revoked tokens should keep their original revocation time");
    }

    [Fact]
    public async Task RevokeAllForUserAsync_ShouldOnlyRevokeNonRevokedTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateTestUser(userId);
        await _context.Users.AddAsync(user);

        var token1 = CreateRefreshToken(userId, "token1");
        var token2 = CreateRefreshToken(userId, "token2");
        token2.IsRevoked = true;
        token2.RevokedAt = DateTime.UtcNow.AddHours(-1);
        
        await _context.RefreshTokens.AddRangeAsync(token1, token2);
        await _context.SaveChangesAsync();

        // Act
        await _sut.RevokeAllForUserAsync(userId);

        // Assert
        var tokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        tokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    private static User CreateTestUser(Guid userId)
    {
        return new User
        {
            Id = userId,
            Email = $"test{userId}@example.com",
            PasswordHash = "hash",
            FullName = "Test User",
            PhoneNumber = "+1234567890",
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
    }

    private static RefreshToken CreateRefreshToken(Guid userId, string token, string? tokenFamily = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            TokenFamily = tokenFamily ?? Guid.NewGuid().ToString("N"),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
