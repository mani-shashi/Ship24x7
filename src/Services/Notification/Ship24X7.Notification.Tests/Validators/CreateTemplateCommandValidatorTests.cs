using FluentAssertions;
using FluentValidation.TestHelper;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Validators;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Tests.Validators;

/// <summary>
/// Validator for CreateTemplateCommandTests ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class CreateTemplateCommandValidatorTests
{
    private readonly CreateTemplateCommandValidator _validator;

    public CreateTemplateCommandValidatorTests()
    {
        _validator = new CreateTemplateCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Booking Confirmation",
            Subject = "Your shipment has been booked",
            BodyTemplate = "Hello {{Name}}, your tracking number is {{TrackingNumber}}.",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name", "TrackingNumber" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_WithNameExceeding200Characters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = new string('A', 201),
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name must not exceed 200 characters");
    }

    [Fact]
    public void Validate_WithEmptySubject_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            Subject = "",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Subject)
            .WithErrorMessage("Subject is required");
    }

    [Fact]
    public void Validate_WithSubjectExceeding500Characters_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            Subject = new string('A', 501),
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Subject)
            .WithErrorMessage("Subject must not exceed 500 characters");
    }

    [Fact]
    public void Validate_WithEmptyBodyTemplate_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BodyTemplate)
            .WithErrorMessage("BodyTemplate is required");
    }

    [Fact]
    public void Validate_WithNullRequiredPlaceholders_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = null!,
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RequiredPlaceholders)
            .WithErrorMessage("RequiredPlaceholders is required");
    }

    [Fact]
    public void Validate_WithEmptyCreatedBy_ShouldHaveValidationError()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            CreatedBy = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreatedBy)
            .WithErrorMessage("CreatedBy is required");
    }

    [Fact]
    public void Validate_WithValidPlaceholdersInTemplate_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Delivery Confirmation",
            Subject = "Shipment {{TrackingNumber}} Delivered",
            BodyTemplate = "Dear {{CustomerName}}, your shipment {{TrackingNumber}} was delivered on {{DeliveryDate}}.",
            Type = TemplateType.DeliveryConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "CustomerName", "TrackingNumber", "DeliveryDate" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultiplePlaceholdersInBody_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Payment Confirmation",
            Subject = "Payment Received",
            BodyTemplate = @"
                Hello {{CustomerName}},
                
                We have received your payment of {{Amount}} {{Currency}} for shipment {{TrackingNumber}}.
                
                Payment ID: {{PaymentId}}
                Date: {{PaymentDate}}
                
                Thank you for your business!
            ",
            Type = TemplateType.PaymentConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "CustomerName", "Amount", "Currency", "TrackingNumber", "PaymentId", "PaymentDate" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyRequiredPlaceholders_ShouldNotHaveErrors()
    {
        // Arrange - Some templates might not require placeholders
        var command = new CreateTemplateCommand
        {
            Name = "Generic Welcome",
            Subject = "Welcome to Ship24X7",
            BodyTemplate = "Thank you for choosing Ship24X7 for your shipping needs.",
            Type = TemplateType.EmailVerification,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = Array.Empty<string>(),
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSpecialCharactersInPlaceholders_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Delay Notification",
            Subject = "Shipment Delayed",
            BodyTemplate = "Your shipment {{TrackingNumber}} has been delayed. Reason: {{DelayReason}}. New ETA: {{NewETA}}.",
            Type = TemplateType.ShipmentDelayed,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "TrackingNumber", "DelayReason", "NewETA" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMaxLengthName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = new string('A', 200),
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithMaxLengthSubject_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateTemplateCommand
        {
            Name = "Test Template",
            Subject = new string('A', 500),
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Subject);
    }
}
