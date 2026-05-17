using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Handlers;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Domain.Entities;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Tests.Handlers;

/// <summary>
/// Handler for processing SendNotificationTests requests. Implements business logic and coordinates with repositories and services.
/// </summary>
public class SendNotificationCommandHandlerTests
{
    private readonly Mock<INotificationLogRepository> _notificationLogRepositoryMock;
    private readonly Mock<INotificationTemplateRepository> _templateRepositoryMock;
    private readonly Mock<INotificationPreferenceRepository> _preferenceRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly Mock<ITemplateRenderer> _templateRendererMock;
    private readonly Mock<ILogger<SendNotificationCommandHandler>> _loggerMock;
    private readonly SendNotificationCommandHandler _sut;

    public SendNotificationCommandHandlerTests()
    {
        _notificationLogRepositoryMock = new Mock<INotificationLogRepository>();
        _templateRepositoryMock = new Mock<INotificationTemplateRepository>();
        _preferenceRepositoryMock = new Mock<INotificationPreferenceRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _smsServiceMock = new Mock<ISmsService>();
        _templateRendererMock = new Mock<ITemplateRenderer>();
        _loggerMock = new Mock<ILogger<SendNotificationCommandHandler>>();

        _sut = new SendNotificationCommandHandler(
            _notificationLogRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _preferenceRepositoryMock.Object,
            _emailServiceMock.Object,
            _smsServiceMock.Object,
            _templateRendererMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithEmailEnabledPreference_ShouldSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);
        var preference = new NotificationPreference
        {
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = true
        };

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientEmail = "test@example.com",
            Channel = NotificationChannel.Email,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string> { { "Name", "John" } }
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _preferenceRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);
        _templateRendererMock.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string t, Dictionary<string, string> d, CancellationToken ct) => t);
        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _emailServiceMock.Verify(x => x.SendEmailAsync("test@example.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationLogRepositoryMock.Verify(x => x.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationLogRepositoryMock.Verify(x => x.UpdateAsync(It.Is<NotificationLog>(n => n.Status == NotificationStatus.Sent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmailDisabledPreference_ShouldNotSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);
        var preference = new NotificationPreference
        {
            UserId = userId,
            EmailEnabled = false,
            SmsEnabled = true
        };

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientEmail = "test@example.com",
            Channel = NotificationChannel.Email,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string>()
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _preferenceRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Guid.Empty);
        _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationLogRepositoryMock.Verify(x => x.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithSmsDisabledPreference_ShouldNotSendSms()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);
        var preference = new NotificationPreference
        {
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = false
        };

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientPhone = "+1234567890",
            Channel = NotificationChannel.SMS,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string>()
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _preferenceRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Guid.Empty);
        _smsServiceMock.Verify(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _notificationLogRepositoryMock.Verify(x => x.AddAsync(It.IsAny<NotificationLog>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoPreference_ShouldSendNotification()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientEmail = "test@example.com",
            Channel = NotificationChannel.Email,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string>()
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _preferenceRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationPreference?)null);
        _templateRendererMock.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string t, Dictionary<string, string> d, CancellationToken ct) => t);
        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _emailServiceMock.Verify(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithSmsEnabledPreference_ShouldSendSms()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);
        var preference = new NotificationPreference
        {
            UserId = userId,
            EmailEnabled = true,
            SmsEnabled = true
        };

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientPhone = "+1234567890",
            Channel = NotificationChannel.SMS,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string>()
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _preferenceRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(preference);
        _templateRendererMock.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string t, Dictionary<string, string> d, CancellationToken ct) => t);
        _smsServiceMock.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _smsServiceMock.Verify(x => x.SendSmsAsync("+1234567890", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _notificationLogRepositoryMock.Verify(x => x.UpdateAsync(It.Is<NotificationLog>(n => n.Status == NotificationStatus.Sent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInactiveTemplate_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);
        template.IsActive = false;

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientEmail = "test@example.com",
            Channel = NotificationChannel.Email,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string>()
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Template {template.Name} is not active");
    }

    [Fact]
    public async Task Handle_WithNonExistentTemplate_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientEmail = "test@example.com",
            Channel = NotificationChannel.Email,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string>()
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationTemplate?)null);

        // Act
        Func<Task> act = async () => await _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Template with ID {templateId} not found");
    }

    [Fact]
    public async Task Handle_WhenEmailServiceFails_ShouldMarkNotificationAsFailed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientEmail = "test@example.com",
            Channel = NotificationChannel.Email,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string>()
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _preferenceRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationPreference?)null);
        _templateRendererMock.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string t, Dictionary<string, string> d, CancellationToken ct) => t);
        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _notificationLogRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<NotificationLog>(n => n.Status == NotificationStatus.Failed && n.ErrorMessage == "Failed to send notification"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailServiceThrowsException_ShouldMarkNotificationAsFailed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);
        var exceptionMessage = "SMTP connection failed";

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientEmail = "test@example.com",
            Channel = NotificationChannel.Email,
            TemplateId = templateId,
            PlaceholderData = new Dictionary<string, string>()
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _preferenceRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationPreference?)null);
        _templateRendererMock.Setup(x => x.RenderAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string t, Dictionary<string, string> d, CancellationToken ct) => t);
        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);
        _notificationLogRepositoryMock.Verify(x => x.UpdateAsync(
            It.Is<NotificationLog>(n => n.Status == NotificationStatus.Failed && n.ErrorMessage == exceptionMessage),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldRenderTemplateWithPlaceholderData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var templateId = Guid.NewGuid();
        var template = CreateActiveTemplate(templateId);
        var placeholderData = new Dictionary<string, string>
        {
            { "Name", "John Doe" },
            { "TrackingNumber", "SHIP24X7-001" }
        };

        var command = new SendNotificationCommand
        {
            UserId = userId,
            RecipientEmail = "test@example.com",
            Channel = NotificationChannel.Email,
            TemplateId = templateId,
            PlaceholderData = placeholderData
        };

        _templateRepositoryMock.Setup(x => x.GetByIdAsync(templateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _preferenceRepositoryMock.Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((NotificationPreference?)null);
        _templateRendererMock.Setup(x => x.RenderAsync(It.IsAny<string>(), placeholderData, It.IsAny<CancellationToken>()))
            .ReturnsAsync("Rendered content");
        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _templateRendererMock.Verify(x => x.RenderAsync(template.Subject, placeholderData, It.IsAny<CancellationToken>()), Times.Once);
        _templateRendererMock.Verify(x => x.RenderAsync(template.BodyTemplate, placeholderData, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static NotificationTemplate CreateActiveTemplate(Guid templateId)
    {
        return new NotificationTemplate
        {
            Id = templateId,
            Name = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            IsActive = true,
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name", "TrackingNumber" },
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid()
        };
    }
}
