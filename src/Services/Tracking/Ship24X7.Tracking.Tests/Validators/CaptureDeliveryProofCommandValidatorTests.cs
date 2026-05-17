using FluentAssertions;
using FluentValidation.TestHelper;
using Ship24X7.Tracking.Application.Commands;
using Ship24X7.Tracking.Application.Validators;
using Xunit;

namespace Ship24X7.Tracking.Tests.Validators;

/// <summary>
/// Validator for CaptureDeliveryProofCommandTests ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class CaptureDeliveryProofCommandValidatorTests
{
    private readonly CaptureDeliveryProofCommandValidator _validator;

    public CaptureDeliveryProofCommandValidatorTests()
    {
        _validator = new CaptureDeliveryProofCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_PassesValidation()
    {
        // Arrange
        var command = new CaptureDeliveryProofCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            ReceivedBy = "John Doe",
            DeliveryDate = DateTime.UtcNow,
            SignatureImageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
            PhotoProofBase64 = Convert.ToBase64String(new byte[] { 4, 5, 6 }),
            Latitude = 45.5m,
            Longitude = -120.75m,
            DeliveredBy = Guid.NewGuid(),
            Notes = "Delivered to front door"
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(90)]
    [InlineData(-90)]
    [InlineData(0)]
    [InlineData(45.5)]
    [InlineData(-45.5)]
    public void Validate_WithValidLatitude_PassesValidation(decimal latitude)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Latitude = latitude;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [InlineData(90.1)]
    [InlineData(91)]
    [InlineData(100)]
    [InlineData(-90.1)]
    [InlineData(-91)]
    [InlineData(-100)]
    public void Validate_WithInvalidLatitude_FailsValidation(decimal latitude)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Latitude = latitude;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Latitude)
            .WithErrorMessage("Latitude must be between -90 and 90");
    }

    [Theory]
    [InlineData(180)]
    [InlineData(-180)]
    [InlineData(0)]
    [InlineData(120.75)]
    [InlineData(-120.75)]
    public void Validate_WithValidLongitude_PassesValidation(decimal longitude)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Longitude = longitude;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Longitude);
    }

    [Theory]
    [InlineData(180.1)]
    [InlineData(181)]
    [InlineData(200)]
    [InlineData(-180.1)]
    [InlineData(-181)]
    [InlineData(-200)]
    public void Validate_WithInvalidLongitude_FailsValidation(decimal longitude)
    {
        // Arrange
        var command = CreateValidCommand();
        command.Longitude = longitude;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Longitude)
            .WithErrorMessage("Longitude must be between -180 and 180");
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
    public void Validate_WithEmptyReceivedBy_FailsValidation(string receivedBy)
    {
        // Arrange
        var command = CreateValidCommand();
        command.ReceivedBy = receivedBy;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReceivedBy)
            .WithErrorMessage("ReceivedBy is required");
    }

    [Fact]
    public void Validate_WithReceivedByExceeding100Characters_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.ReceivedBy = new string('a', 101);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReceivedBy)
            .WithErrorMessage("ReceivedBy cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_WithFutureDeliveryDate_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.DeliveryDate = DateTime.UtcNow.AddHours(2);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeliveryDate)
            .WithErrorMessage("DeliveryDate cannot be in the future");
    }

    [Fact]
    public void Validate_WithDeliveryDateWithinOneHour_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.DeliveryDate = DateTime.UtcNow.AddMinutes(30);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DeliveryDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptySignatureImage_FailsValidation(string signatureImage)
    {
        // Arrange
        var command = CreateValidCommand();
        command.SignatureImageBase64 = signatureImage;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SignatureImageBase64)
            .WithErrorMessage("Signature image is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WithEmptyPhotoProof_FailsValidation(string photoProof)
    {
        // Arrange
        var command = CreateValidCommand();
        command.PhotoProofBase64 = photoProof;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PhotoProofBase64)
            .WithErrorMessage("Photo proof is required");
    }

    [Fact]
    public void Validate_WithEmptyDeliveredBy_FailsValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.DeliveredBy = Guid.Empty;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DeliveredBy)
            .WithErrorMessage("DeliveredBy is required");
    }

    [Fact]
    public void Validate_WithAllBoundaryCoordinates_PassesValidation()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Latitude = 90m;
        command.Longitude = 180m;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Latitude);
        result.ShouldNotHaveValidationErrorFor(x => x.Longitude);
    }

    private CaptureDeliveryProofCommand CreateValidCommand()
    {
        return new CaptureDeliveryProofCommand
        {
            ShipmentId = Guid.NewGuid(),
            TrackingNumber = "SHIP24X7-20260414001",
            ReceivedBy = "John Doe",
            DeliveryDate = DateTime.UtcNow.AddMinutes(-10),
            SignatureImageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 }),
            PhotoProofBase64 = Convert.ToBase64String(new byte[] { 4, 5, 6 }),
            Latitude = 45.5m,
            Longitude = -120.75m,
            DeliveredBy = Guid.NewGuid(),
            Notes = "Test notes"
        };
    }
}
