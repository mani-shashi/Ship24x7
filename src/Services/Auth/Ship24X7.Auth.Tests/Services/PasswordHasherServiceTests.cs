using FluentAssertions;
using Ship24X7.Auth.Infrastructure.Services;

namespace Ship24X7.Auth.Tests.Services;

public class PasswordHasherServiceTests
{
    private readonly PasswordHasherService _sut;

    public PasswordHasherServiceTests()
    {
        _sut = new PasswordHasherService();
    }

    [Fact]
    public void HashPassword_ShouldReturnNonEmptyHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _sut.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_ShouldReturnDifferentHashesForSamePassword()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _sut.HashPassword(password);
        var hash2 = _sut.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "each hash should have a unique salt");
    }

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword456!";
        var hash = _sut.HashPassword(password);

        // Act
        var result = _sut.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a")]
    [InlineData("VeryLongPasswordThatExceedsNormalLimitsButShouldStillBeHashedCorrectly123456789!@#$%^&*()")]
    public void HashPassword_WithVariousInputs_ShouldProduceValidHash(string password)
    {
        // Act
        var hash = _sut.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        var verified = _sut.VerifyPassword(password, hash);
        verified.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithEmptyHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var emptyHash = string.Empty;

        // Act
        var result = _sut.VerifyPassword(password, emptyHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithInvalidHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var invalidHash = "InvalidHashString";

        // Act
        var result = _sut.VerifyPassword(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_ShouldUsePBKDF2Algorithm()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = _sut.HashPassword(password);

        // Assert - PBKDF2 hashes start with specific format
        hash.Should().StartWith("AQ", "PBKDF2 hashes from ASP.NET Identity start with AQ");
    }
}
