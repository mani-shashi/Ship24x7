using FluentAssertions;
using FluentValidation.TestHelper;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Validators;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Tests.Validators;

/// <summary>
/// Validator for UpdateTemplateCommandTests ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class UpdateTemplateCommandValidatorTests
{
    private readonly UpdateTemplateCommandValidator _validator;

    public UpdateTemplateCommandValidatorTests()
    {
        _validator = new UpdateTemplateCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Updated Booking Confirmation",
            Subject = "Your shipment has been booked - Updated",
            BodyTemplate = "Hello {{Name}}, your tracking number is {{TrackingNumber}}.",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name", "TrackingNumber" },
            IsActive = true,
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTemplateId_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.Empty,
            Name = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TemplateId)
            .WithErrorMessage("TemplateId is required");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            UpdatedBy = Guid.NewGuid()
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
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = new string('A', 201),
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            UpdatedBy = Guid.NewGuid()
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
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Test Template",
            Subject = "",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            UpdatedBy = Guid.NewGuid()
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
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Test Template",
            Subject = new string('A', 501),
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            UpdatedBy = Guid.NewGuid()
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
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            UpdatedBy = Guid.NewGuid()
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
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = null!,
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RequiredPlaceholders)
            .WithErrorMessage("RequiredPlaceholders is required");
    }

    [Fact]
    public void Validate_WithEmptyUpdatedBy_ShouldHaveValidationError()
    {
        // Arrange
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Test Template",
            Subject = "Test Subject",
            BodyTemplate = "Test Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            UpdatedBy = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UpdatedBy)
            .WithErrorMessage("UpdatedBy is required");
    }

    [Fact]
    public void Validate_WithUpdatedPlaceholders_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Updated Delivery Confirmation",
            Subject = "Shipment {{TrackingNumber}} Delivered - Updated",
            BodyTemplate = "Dear {{CustomerName}}, your shipment {{TrackingNumber}} was delivered on {{DeliveryDate}} at {{DeliveryTime}}.",
            Type = TemplateType.DeliveryConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "CustomerName", "TrackingNumber", "DeliveryDate", "DeliveryTime" },
            IsActive = true,
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithIsActiveFalse_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Deprecated Template",
            Subject = "Old Subject",
            BodyTemplate = "Old Body",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "Name" },
            IsActive = false,
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithComplexPlaceholderStructure_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateTemplateCommand
        {
            TemplateId = Guid.NewGuid(),
            Name = "Complex Notification",
            Subject = "Order Update",
            BodyTemplate = @"
                Dear {{CustomerName}},
                
                Order Details:
                - Tracking: {{TrackingNumber}}
                - Origin: {{OriginCity}}, {{OriginState}}
                - Destination: {{DestinationCity}}, {{DestinationState}}
                - Weight: {{Weight}} kg
                - Cost: {{Currency}} {{Amount}}
                
                Estimated Delivery: {{EstimatedDeliveryDate}}
            ",
            Type = TemplateType.BookingConfirmation,
            Channel = NotificationChannel.Email,
            RequiredPlaceholders = new[] { "CustomerName", "TrackingNumber", "OriginCity", "OriginState", "DestinationCity", "DestinationState", "Weight", "Currency", "Amount", "EstimatedDeliveryDate" },
            IsActive = true,
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
