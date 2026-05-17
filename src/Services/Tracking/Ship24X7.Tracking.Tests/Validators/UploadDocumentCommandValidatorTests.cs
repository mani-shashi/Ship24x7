using FluentAssertions;
using FluentValidation.TestHelper;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.Validators;
using Ship24X7.Tracking.Domain.Enums;
using Xunit;

namespace Ship24X7.Tracking.Tests.Validators;

/// <summary>
/// Validator for UploadDocumentCommandTests ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class UploadDocumentCommandValidatorTests
{
    private readonly UploadDocumentCommandValidator _validator;

    public UploadDocumentCommandValidatorTests()
    {
        _validator = new UploadDocumentCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        // Arrange
        var command = new UploadDocumentCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            FileName = "invoice.pdf",
            FileContent = new byte[1024], // 1 KB
            ContentType = "application/pdf",
            DocumentType = DocumentType.Invoice,
            UploadedBy = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("image/jpeg")]
    [InlineData("image/jpg")]
    [InlineData("image/png")]
    public void Validate_WithAllowedContentTypes_PassesValidation(string contentType)
    {
        // Arrange
        var command = CreateValidCommand();
        command.ContentType = contentType;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ContentType);
    }

    [Theory]
    [InlineData("application/msword")]
    [InlineData("text/plain")]
    [InlineData("application/zip")]
    [InlineData("image/gif")]
    [InlineData("video/mp4")]
    public void Validate_WithDisallowedContentTypes_FailsValidation(string contentType)
    {
        // Arrange
        var command = CreateValidCommand();
        command.ContentType = contentType;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ContentType)
            .WithErrorMessage("File type must be PDF, JPG, or PNG");
    }

    [Fact]
    public void Validate_WithFileSizeExactly10MB_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.FileContent = new byte[10 * 1024 * 1024]; // Exactly 10 MB

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FileContent);
    }

    [Fact]
    public void Validate_WithFileSizeUnder10MB_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.FileContent = new byte[5 * 1024 * 1024]; // 5 MB

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FileContent);
    }

    [Fact]
    public void Validate_WithFileSizeOver10MB_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.FileContent = new byte[(10 * 1024 * 1024) + 1]; // 10 MB + 1 byte

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileContent)
            .WithErrorMessage("File size cannot exceed 10 MB");
    }

    [Fact]
    public void Validate_WithEmptyFileContent_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.FileContent = Array.Empty<byte>();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileContent)
            .WithErrorMessage("FileContent is required");
    }

    [Fact]
    public void Validate_WithEmptyShipmentId_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.ShipmentId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShipmentId)
            .WithErrorMessage("ShipmentId is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyTrackingNumber_FailsValidation(string trackingNumber)
    {
        // Arrange
        var command = CreateValidCommand();
        command.TrackingNumber = trackingNumber;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TrackingNumber)
            .WithErrorMessage("TrackingNumber is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyFileName_FailsValidation(string fileName)
    {
        // Arrange
        var command = CreateValidCommand();
        command.FileName = fileName;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("FileName is required");
    }

    [Fact]
    public void Validate_WithFileNameExceeding255Characters_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.FileName = new string('a', 256) + ".pdf";

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FileName)
            .WithErrorMessage("FileName cannot exceed 255 characters");
    }

    [Fact]
    public void Validate_WithEmptyUploadedBy_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.UploadedBy = Guid.Empty;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UploadedBy)
            .WithErrorMessage("UploadedBy is required");
    }

    [Theory]
    [InlineData("APPLICATION/PDF")]
    [InlineData("Image/JPEG")]
    [InlineData("IMAGE/PNG")]
    public void Validate_WithMixedCaseContentType_PassesValidation(string contentType)
    {
        // Arrange
        var command = CreateValidCommand();
        command.ContentType = contentType;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ContentType);
    }

    private UploadDocumentCommand CreateValidCommand()
    {
        return new UploadDocumentCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            FileName = "document.pdf",
            FileContent = new byte[1024],
            ContentType = "application/pdf",
            DocumentType = DocumentType.UserUpload,
            UploadedBy = Guid.NewGuid()
        };
    }
}
