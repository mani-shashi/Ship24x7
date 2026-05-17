using FluentAssertions;
using Moq;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Handlers;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.Tests.Handlers;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IMfaService> _mfaServiceMock;
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _mfaServiceMock = new Mock<IMfaService>();

        _sut = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _mfaServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.RequiresMfa.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(command.Email))
            .ReturnsAsync((User?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "WrongPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldIncrementFailedLoginAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        user.FailedLoginAttempts = 2;
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "WrongPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        try
        {
            await _sut.Handle(command, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
            // Expected
        }

        // Assert
        user.FailedLoginAttempts.Should().Be(3);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_After5FailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var user = CreateTestUser();
        user.FailedLoginAttempts = 4; // This will be the 5th attempt
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "WrongPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(false);

        // Act
        try
        {
            await _sut.Handle(command, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
            // Expected
        }

        // Assert
        user.FailedLoginAttempts.Should().Be(5);
        user.IsLocked.Should().BeTrue();
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLockedAccount_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsLocked = true;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(10);
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is locked");
    }

    [Fact]
    public async Task Handle_WithExpiredLockout_ShouldAllowLogin()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsLocked = true;
        user.LockoutEnd = DateTime.UtcNow.AddMinutes(-1); // Expired lockout
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithUnverifiedEmail_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        user.EmailVerified = false;
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Email not verified");
    }

    [Fact]
    public async Task Handle_WithInactiveAccount_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        user.IsActive = false;
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Account is not active");
    }

    [Fact]
    public async Task Handle_WithMfaEnabled_AndNoMfaCode_ShouldReturnRequiresMfa()
    {
        // Arrange
        var user = CreateTestUser();
        user.MfaSettings = new MfaSettings
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsEnabled = true,
            TotpSecret = "TESTSECRET",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RequiresMfa.Should().BeTrue();
        result.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMfaEnabled_AndValidMfaCode_ShouldReturnTokens()
    {
        // Arrange
        var user = CreateTestUser();
        user.MfaSettings = new MfaSettings
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsEnabled = true,
            TotpSecret = "TESTSECRET",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!",
            MfaCode = "123456"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _mfaServiceMock.Setup(x => x.ValidateTotpCode(user.MfaSettings.TotpSecret, command.MfaCode))
            .Returns(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
        result.RequiresMfa.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithMfaEnabled_AndInvalidMfaCode_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        user.MfaSettings = new MfaSettings
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsEnabled = true,
            TotpSecret = "TESTSECRET",
            FailedAttempts = 0,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!",
            MfaCode = "000000"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _mfaServiceMock.Setup(x => x.ValidateTotpCode(user.MfaSettings.TotpSecret, command.MfaCode))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid MFA code");
    }

    [Fact]
    public async Task Handle_WithMfaEnabled_AndInvalidMfaCode_ShouldIncrementFailedMfaAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        user.MfaSettings = new MfaSettings
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsEnabled = true,
            TotpSecret = "TESTSECRET",
            FailedAttempts = 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!",
            MfaCode = "000000"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _mfaServiceMock.Setup(x => x.ValidateTotpCode(user.MfaSettings.TotpSecret, command.MfaCode))
            .Returns(false);

        // Act
        try
        {
            await _sut.Handle(command, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
            // Expected
        }

        // Assert
        user.MfaSettings.FailedAttempts.Should().Be(2);
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_After3FailedMfaAttempts_ShouldLockMfa()
    {
        // Arrange
        var user = CreateTestUser();
        user.MfaSettings = new MfaSettings
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsEnabled = true,
            TotpSecret = "TESTSECRET",
            FailedAttempts = 2, // This will be the 3rd attempt
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!",
            MfaCode = "000000"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _mfaServiceMock.Setup(x => x.ValidateTotpCode(user.MfaSettings.TotpSecret, command.MfaCode))
            .Returns(false);

        // Act
        try
        {
            await _sut.Handle(command, CancellationToken.None);
        }
        catch (UnauthorizedAccessException)
        {
            // Expected
        }

        // Assert
        user.MfaSettings.FailedAttempts.Should().Be(3);
        user.MfaSettings.LockedUntil.Should().NotBeNull();
        user.MfaSettings.LockedUntil.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(5), TimeSpan.FromSeconds(5));
        _userRepositoryMock.Verify(x => x.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WithLockedMfa_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = CreateTestUser();
        user.MfaSettings = new MfaSettings
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            IsEnabled = true,
            TotpSecret = "TESTSECRET",
            FailedAttempts = 3,
            LockedUntil = DateTime.UtcNow.AddMinutes(3),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!",
            MfaCode = "123456"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("MFA is temporarily locked");
    }

    [Fact]
    public async Task Handle_WithSuccessfulLogin_ShouldResetFailedLoginAttempts()
    {
        // Arrange
        var user = CreateTestUser();
        user.FailedLoginAttempts = 3;
        
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        user.IsLocked.Should().BeFalse();
        user.LockoutEnd.Should().BeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_WithSuccessfulLogin_ShouldStoreRefreshToken()
    {
        // Arrange
        var user = CreateTestUser();
        var command = new LoginCommand
        {
            Email = user.Email,
            Password = "ValidPassword123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailWithRolesAsync(user.Email))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash))
            .Returns(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>()))
            .Returns("access-token");
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _refreshTokenRepositoryMock.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == user.Id &&
            rt.Token == "refresh-token" &&
            rt.ExpiresAt > DateTime.UtcNow &&
            !rt.IsRevoked
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
}
