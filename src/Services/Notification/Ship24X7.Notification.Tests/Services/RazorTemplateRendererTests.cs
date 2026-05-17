using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Ship24X7.Notification.Infrastructure.Services;

namespace Ship24X7.Notification.Tests.Services;

/// <summary>
/// RazorTemplateRendererTests implementation. Provides functionality for the application.
/// </summary>
public class RazorTemplateRendererTests
{
    private readonly RazorTemplateRenderer _sut;
    private readonly Mock<ILogger<RazorTemplateRenderer>> _loggerMock;

    public RazorTemplateRendererTests()
    {
        _loggerMock = new Mock<ILogger<RazorTemplateRenderer>>();
        _sut = new RazorTemplateRenderer(_loggerMock.Object);
    }

    [Fact]
    public async Task RenderAsync_WithValidPlaceholders_ShouldReplaceAllPlaceholders()
    {
        // Arrange
        var template = "Hello {{Name}}, your tracking number is {{TrackingNumber}}.";
        var placeholderData = new Dictionary<string, string>
        {
            { "Name", "John Doe" },
            { "TrackingNumber", "SHIP24X7-20260414001" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Hello John Doe, your tracking number is SHIP24X7-20260414001.");
    }

    [Fact]
    public async Task RenderAsync_WithMultiplePlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var template = "Order {{OrderId}} for {{CustomerName}} - Total: {{Amount}} {{Currency}}";
        var placeholderData = new Dictionary<string, string>
        {
            { "OrderId", "12345" },
            { "CustomerName", "Jane Smith" },
            { "Amount", "1500.00" },
            { "Currency", "INR" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Order 12345 for Jane Smith - Total: 1500.00 INR");
    }

    [Fact]
    public async Task RenderAsync_WithNoPlaceholders_ShouldReturnOriginalTemplate()
    {
        // Arrange
        var template = "This is a simple message with no placeholders.";
        var placeholderData = new Dictionary<string, string>();

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be(template);
    }

    [Fact]
    public async Task RenderAsync_WithMissingPlaceholderData_ShouldLeaveUnreplacedAndLogWarning()
    {
        // Arrange
        var template = "Hello {{Name}}, your order {{OrderId}} is ready.";
        var placeholderData = new Dictionary<string, string>
        {
            { "Name", "John Doe" }
            // OrderId is missing
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Hello John Doe, your order {{OrderId}} is ready.");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("unreplaced placeholders")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RenderAsync_WithEmptyTemplate_ShouldReturnEmptyString()
    {
        // Arrange
        var template = "";
        var placeholderData = new Dictionary<string, string>
        {
            { "Name", "John Doe" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task RenderAsync_WithSpecialCharactersInPlaceholderValue_ShouldReplaceCorrectly()
    {
        // Arrange
        var template = "Message: {{Content}}";
        var placeholderData = new Dictionary<string, string>
        {
            { "Content", "Special chars: <>&\"'@#$%^&*()" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Message: Special chars: <>&\"'@#$%^&*()");
    }

    [Fact]
    public async Task RenderAsync_WithRepeatedPlaceholder_ShouldReplaceAllOccurrences()
    {
        // Arrange
        var template = "{{Name}} ordered item A. {{Name}} also ordered item B. Thank you, {{Name}}!";
        var placeholderData = new Dictionary<string, string>
        {
            { "Name", "Alice" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Alice ordered item A. Alice also ordered item B. Thank you, Alice!");
    }

    [Fact]
    public async Task RenderAsync_WithCaseSensitivePlaceholders_ShouldMatchExactCase()
    {
        // Arrange
        var template = "Hello {{name}}, your NAME is {{NAME}}.";
        var placeholderData = new Dictionary<string, string>
        {
            { "name", "lowercase" },
            { "NAME", "UPPERCASE" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Hello lowercase, your NAME is UPPERCASE.");
    }

    [Fact]
    public async Task RenderAsync_WithNumericPlaceholderValues_ShouldReplaceCorrectly()
    {
        // Arrange
        var template = "Total: {{Amount}}, Quantity: {{Quantity}}";
        var placeholderData = new Dictionary<string, string>
        {
            { "Amount", "1234.56" },
            { "Quantity", "10" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Total: 1234.56, Quantity: 10");
    }

    [Fact]
    public async Task RenderAsync_WithDatePlaceholderValues_ShouldReplaceCorrectly()
    {
        // Arrange
        var template = "Delivery date: {{DeliveryDate}}";
        var placeholderData = new Dictionary<string, string>
        {
            { "DeliveryDate", "2026-04-15" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Delivery date: 2026-04-15");
    }

    [Fact]
    public async Task RenderAsync_WithEmptyPlaceholderValue_ShouldReplaceWithEmptyString()
    {
        // Arrange
        var template = "Name: {{Name}}, Notes: {{Notes}}";
        var placeholderData = new Dictionary<string, string>
        {
            { "Name", "John" },
            { "Notes", "" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Name: John, Notes: ");
    }

    [Fact]
    public async Task RenderAsync_WithMultilinePlaceholderValue_ShouldReplaceCorrectly()
    {
        // Arrange
        var template = "Address:\n{{Address}}";
        var placeholderData = new Dictionary<string, string>
        {
            { "Address", "123 Main St\nApt 4B\nNew York, NY 10001" }
        };

        // Act
        var result = await _sut.RenderAsync(template, placeholderData);

        // Assert
        result.Should().Be("Address:\n123 Main St\nApt 4B\nNew York, NY 10001");
    }
}
