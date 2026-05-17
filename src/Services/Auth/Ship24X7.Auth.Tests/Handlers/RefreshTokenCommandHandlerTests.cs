using FluentAssertions;
using Moq;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Handlers;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Tests.Handlers;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly RefreshTokenCommandHandler _sut;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _tokenServiceMock = new Mock<ITokenService>();

        _sut = new RefreshTokenCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("new-access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new-access-token");
        result.RefreshToken.Should().Be("new-refresh-token");
        result.RequiresMfa.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new RefreshTokenCommand { RefreshToken = "invalid-token" };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(command.RefreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token");
    }

    [Fact]
    public async Task Handle_WithRevokedRefreshToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow.AddMinutes(-5);
        
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token");
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(-1);
        
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid or expired refresh token");
    }

    [Fact]
    public async Task Handle_WithReplayedToken_ShouldRevokeTokenFamilyAndThrowException()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        refreshToken.ReplacedByToken = "new-token-that-replaced-this";
        
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Token replay detected");
        
        _refreshTokenRepositoryMock.Verify(
            x => x.RevokeTokenFamilyAsync(refreshToken.TokenFamily), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsActive = false;
        var refreshToken = CreateTestRefreshToken(user.Id);
        
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("User not found or inactive");
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldRevokeOldToken()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("new-access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        refreshToken.ReplacedByToken.Should().Be("new-refresh-token");
        _refreshTokenRepositoryMock.Verify(x => x.UpdateAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldStoreNewRefreshToken()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("new-access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == user.Id &&
            rt.Token == "new-refresh-token" &&
            rt.ExpiresAt > DateTime.UtcNow &&
            !rt.IsRevoked &&
            rt.TokenFamily == refreshToken.TokenFamily
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldMaintainTokenFamily()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        var originalTokenFamily = refreshToken.TokenFamily;
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("new-access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.TokenFamily == originalTokenFamily
        )), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldGenerateNewAccessTokenWithUserClaims()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("new-access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _tokenServiceMock.Verify(x => x.GenerateAccessToken(
            user.Id,
            user.Email,
            It.Is<IEnumerable<string>>(roles => roles.Contains("Customer")),
            It.IsAny<IEnumerable<string>>()
        ), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldSetCorrectExpiryForNewRefreshToken()
    {
        // Arrange
        var user = CreateTestUser();
        var refreshToken = CreateTestRefreshToken(user.Id);
        var command = new RefreshTokenCommand { RefreshToken = refreshToken.Token };

        _refreshTokenRepositoryMock.Setup(x => x.GetByTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);
        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("new-access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.ExpiresAt > DateTime.UtcNow.AddDays(6) &&
            rt.ExpiresAt < DateTime.UtcNow.AddDays(8)
        )), Times.Once);
    }

    private static User CreateTestUser()
    {
        var userId = Guid.NewGuid();
        return new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hashed-password",
            FullName = "Test User",
            PhoneNumber = "+1234567890",
            EmailVerified = true,
            IsActive = true,
            IsLocked = false,
            FailedLoginAttempts = 0,
            UserRoles = new List<UserRole>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    RoleId = Guid.NewGuid(),
                    Role = new Role
                    {
                        Id = Guid.NewGuid(),
                        Name = "Customer",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    },
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                }
            },
            UserClaims = new List<UserClaim>(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
    }

    private static RefreshToken CreateTestRefreshToken(Guid userId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            TokenFamily = Guid.NewGuid().ToString("N"),
            User = new User
            {
                Id = userId,
                Email = "test@example.com",
                PasswordHash = "hashed-password",
                FullName = "Test User",
                PhoneNumber = "+1234567890",
                EmailVerified = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            },
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
    }
}
