using FluentAssertions;
using OtpNet;
using Ship24X7.Auth.Infrastructure.Services;

namespace Ship24X7.Auth.Tests.Services;

public class TotpMfaServiceTests
{
    private readonly TotpMfaService _sut;

    public TotpMfaServiceTests()
    {
        _sut = new TotpMfaService();
    }

    [Fact]
    public void GenerateTotpSecret_ShouldReturnNonEmptySecret()
    {
        // Act
        var secret = _sut.GenerateTotpSecret();

        // Assert
        secret.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateTotpSecret_ShouldReturnDifferentSecretsEachTime()
    {
        // Act
        var secret1 = _sut.GenerateTotpSecret();
        var secret2 = _sut.GenerateTotpSecret();

        // Assert
        secret1.Should().NotBe(secret2);
    }

    [Fact]
    public void GenerateTotpSecret_ShouldReturnBase32EncodedString()
    {
        // Act
        var secret = _sut.GenerateTotpSecret();

        // Assert
        // Base32 strings only contain A-Z and 2-7
        secret.Should().MatchRegex("^[A-Z2-7]+$");
    }

    [Fact]
    public void GenerateQrCodeUri_ShouldReturnValidOtpAuthUri()
    {
        // Arrange
        var email = "test@example.com";
        var secret = _sut.GenerateTotpSecret();

        // Act
        var uri = _sut.GenerateQrCodeUri(email, secret);

        // Assert
        uri.Should().StartWith("otpauth://totp/");
        uri.Should().Contain(email);
        uri.Should().Contain($"secret={secret}");
        uri.Should().Contain("issuer=Ship24X7");
    }

    [Fact]
    public void GenerateQrCodeUri_ShouldIncludeIssuerInPath()
    {
        // Arrange
        var email = "test@example.com";
        var secret = _sut.GenerateTotpSecret();

        // Act
        var uri = _sut.GenerateQrCodeUri(email, secret);

        // Assert
        uri.Should().Contain("Ship24X7:");
    }

    [Fact]
    public void ValidateTotpCode_WithValidCode_ShouldReturnTrue()
    {
        // Arrange
        var secret = _sut.GenerateTotpSecret();
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        var validCode = totp.ComputeTotp();

        // Act
        var result = _sut.ValidateTotpCode(secret, validCode);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateTotpCode_WithInvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var secret = _sut.GenerateTotpSecret();
        var invalidCode = "000000";

        // Act
        var result = _sut.ValidateTotpCode(secret, invalidCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateTotpCode_WithEmptyCode_ShouldReturnFalse()
    {
        // Arrange
        var secret = _sut.GenerateTotpSecret();
        var emptyCode = string.Empty;

        // Act
        var result = _sut.ValidateTotpCode(secret, emptyCode);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    public void ValidateTotpCode_WithInvalidCodeFormat_ShouldReturnFalse(string invalidCode)
    {
        // Arrange
        var secret = _sut.GenerateTotpSecret();

        // Act
        var result = _sut.ValidateTotpCode(secret, invalidCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateTotpCode_ShouldAcceptCodesWithinTimeWindow()
    {
        // Arrange
        var secret = _sut.GenerateTotpSecret();
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        
        // Generate code for current time
        var currentCode = totp.ComputeTotp();

        // Act - Validate immediately
        var result = _sut.ValidateTotpCode(secret, currentCode);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GenerateBackupCodes_ShouldReturnDefaultCountOf10()
    {
        // Act
        var codes = _sut.GenerateBackupCodes();

        // Assert
        codes.Should().HaveCount(10);
    }

    [Fact]
    public void GenerateBackupCodes_WithCustomCount_ShouldReturnSpecifiedCount()
    {
        // Arrange
        var count = 5;

        // Act
        var codes = _sut.GenerateBackupCodes(count);

        // Assert
        codes.Should().HaveCount(count);
    }

    [Fact]
    public void GenerateBackupCodes_ShouldReturnUniqueCodesEachTime()
    {
        // Act
        var codes1 = _sut.GenerateBackupCodes();
        var codes2 = _sut.GenerateBackupCodes();

        // Assert
        codes1.Should().NotBeEquivalentTo(codes2);
    }

    [Fact]
    public void GenerateBackupCodes_ShouldReturn8CharacterCodes()
    {
        // Act
        var codes = _sut.GenerateBackupCodes();

        // Assert
        codes.Should().AllSatisfy(code => code.Length.Should().Be(8));
    }

    [Fact]
    public void GenerateBackupCodes_ShouldReturnUppercaseCodes()
    {
        // Act
        var codes = _sut.GenerateBackupCodes();

        // Assert
        codes.Should().AllSatisfy(code => code.Should().MatchRegex("^[A-Z0-9]+$"));
    }

    [Fact]
    public void GenerateBackupCodes_ShouldReturnAllUniqueCodes()
    {
        // Act
        var codes = _sut.GenerateBackupCodes(20);

        // Assert
        codes.Distinct().Should().HaveCount(20);
    }

    [Fact]
    public void ValidateTotpCode_WithPreviousTimeWindowCode_ShouldReturnTrue()
    {
        // Arrange
        var secret = _sut.GenerateTotpSecret();
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        
        // Generate code for previous time window (30 seconds ago)
        var previousTime = DateTime.UtcNow.AddSeconds(-30);
        var previousCode = totp.ComputeTotp(previousTime);

        // Act
        var result = _sut.ValidateTotpCode(secret, previousCode);

        // Assert
        result.Should().BeTrue("the service uses a verification window of 2 periods before and after");
    }

    [Fact]
    public void ValidateTotpCode_WithNextTimeWindowCode_ShouldReturnTrue()
    {
        // Arrange
        var secret = _sut.GenerateTotpSecret();
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        
        // Generate code for next time window (30 seconds ahead)
        var nextTime = DateTime.UtcNow.AddSeconds(30);
        var nextCode = totp.ComputeTotp(nextTime);

        // Act
        var result = _sut.ValidateTotpCode(secret, nextCode);

        // Assert
        result.Should().BeTrue("the service uses a verification window of 2 periods before and after");
    }
}
