using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Ship24X7.Payment.Infrastructure.Services;

namespace Ship24X7.Payment.Tests.Services;

/// <summary>
/// Tests for HMAC-SHA256 signature verification
/// **Validates: Requirements 17.2**
/// </summary>
public class HmacSignatureValidatorTests
{
    private readonly HmacSignatureValidator _sut;
    private const string TestKeySecret = "test_key_secret_12345";
    private const string TestWebhookSecret = "test_webhook_secret_67890";

    public HmacSignatureValidatorTests()
    {
        var configData = new Dictionary<string, string>
        {
            { "Razorpay:KeySecret", TestKeySecret },
            { "Razorpay:WebhookSecret", TestWebhookSecret }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        _sut = new HmacSignatureValidator(configuration);
    }

    [Fact]
    public void ValidatePaymentSignature_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var orderId = "order_MHkLZjQqvXZKqp";
        var paymentId = "pay_MHkLZjQqvXZKqp";
        var message = $"{orderId}|{paymentId}";
        var validSignature = ComputeHmacSha256(message, TestKeySecret);

        // Act
        var result = _sut.ValidatePaymentSignature(orderId, paymentId, validSignature);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidatePaymentSignature_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        var orderId = "order_MHkLZjQqvXZKqp";
        var paymentId = "pay_MHkLZjQqvXZKqp";
        var invalidSignature = "invalid_signature_12345";

        // Act
        var result = _sut.ValidatePaymentSignature(orderId, paymentId, invalidSignature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePaymentSignature_WithTamperedOrderId_ReturnsFalse()
    {
        // Arrange
        var orderId = "order_MHkLZjQqvXZKqp";
        var paymentId = "pay_MHkLZjQqvXZKqp";
        var message = $"{orderId}|{paymentId}";
        var validSignature = ComputeHmacSha256(message, TestKeySecret);

        var tamperedOrderId = "order_TAMPERED123456";

        // Act
        var result = _sut.ValidatePaymentSignature(tamperedOrderId, paymentId, validSignature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePaymentSignature_WithTamperedPaymentId_ReturnsFalse()
    {
        // Arrange
        var orderId = "order_MHkLZjQqvXZKqp";
        var paymentId = "pay_MHkLZjQqvXZKqp";
        var message = $"{orderId}|{paymentId}";
        var validSignature = ComputeHmacSha256(message, TestKeySecret);

        var tamperedPaymentId = "pay_TAMPERED123456";

        // Act
        var result = _sut.ValidatePaymentSignature(orderId, tamperedPaymentId, validSignature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePaymentSignature_IsCaseInsensitive()
    {
        // Arrange
        var orderId = "order_MHkLZjQqvXZKqp";
        var paymentId = "pay_MHkLZjQqvXZKqp";
        var message = $"{orderId}|{paymentId}";
        var validSignature = ComputeHmacSha256(message, TestKeySecret);
        var uppercaseSignature = validSignature.ToUpperInvariant();

        // Act
        var result = _sut.ValidatePaymentSignature(orderId, paymentId, uppercaseSignature);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidatePaymentSignature_WithEmptyOrderId_ReturnsFalse()
    {
        // Arrange
        var orderId = "";
        var paymentId = "pay_MHkLZjQqvXZKqp";
        var signature = "some_signature";

        // Act
        var result = _sut.ValidatePaymentSignature(orderId, paymentId, signature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidatePaymentSignature_WithEmptyPaymentId_ReturnsFalse()
    {
        // Arrange
        var orderId = "order_MHkLZjQqvXZKqp";
        var paymentId = "";
        var signature = "some_signature";

        // Act
        var result = _sut.ValidatePaymentSignature(orderId, paymentId, signature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateWebhookSignature_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var payload = "{\"event\":\"payment.captured\",\"payload\":{\"payment\":{\"entity\":{\"id\":\"pay_123\"}}}}";
        var validSignature = ComputeHmacSha256(payload, TestWebhookSecret);

        // Act
        var result = _sut.ValidateWebhookSignature(payload, validSignature);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateWebhookSignature_WithInvalidSignature_ReturnsFalse()
    {
        // Arrange
        var payload = "{\"event\":\"payment.captured\",\"payload\":{\"payment\":{\"entity\":{\"id\":\"pay_123\"}}}}";
        var invalidSignature = "invalid_webhook_signature";

        // Act
        var result = _sut.ValidateWebhookSignature(payload, invalidSignature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateWebhookSignature_WithTamperedPayload_ReturnsFalse()
    {
        // Arrange
        var payload = "{\"event\":\"payment.captured\",\"payload\":{\"payment\":{\"entity\":{\"id\":\"pay_123\"}}}}";
        var validSignature = ComputeHmacSha256(payload, TestWebhookSecret);

        var tamperedPayload = "{\"event\":\"payment.captured\",\"payload\":{\"payment\":{\"entity\":{\"id\":\"pay_999\"}}}}";

        // Act
        var result = _sut.ValidateWebhookSignature(tamperedPayload, validSignature);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateWebhookSignature_IsCaseInsensitive()
    {
        // Arrange
        var payload = "{\"event\":\"payment.captured\"}";
        var validSignature = ComputeHmacSha256(payload, TestWebhookSecret);
        var uppercaseSignature = validSignature.ToUpperInvariant();

        // Act
        var result = _sut.ValidateWebhookSignature(payload, uppercaseSignature);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithMissingKeySecret_ThrowsException()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Razorpay:WebhookSecret", TestWebhookSecret }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configData!).Build();

        // Act
        Action act = () => new HmacSignatureValidator(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Razorpay KeySecret not configured");
    }

    [Fact]
    public void Constructor_WithMissingWebhookSecret_ThrowsException()
    {
        // Arrange
        var configData = new Dictionary<string, string>
        {
            { "Razorpay:KeySecret", TestKeySecret }
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(configData!).Build();

        // Act
        Action act = () => new HmacSignatureValidator(config);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Razorpay WebhookSecret not configured");
    }

    private static string ComputeHmacSha256(string message, string secret)
    {
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(secret);
        var messageBytes = System.Text.Encoding.UTF8.GetBytes(message);

        using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
