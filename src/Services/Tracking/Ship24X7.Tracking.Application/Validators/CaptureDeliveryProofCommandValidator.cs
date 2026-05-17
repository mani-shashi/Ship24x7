using FluentValidation;
using Ship24X7.Tracking.Application.Commands;

namespace Ship24X7.Tracking.Application.Validators;

/// <summary>
/// Validator for CaptureDeliveryProofCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class CaptureDeliveryProofCommandValidator : AbstractValidator<CaptureDeliveryProofCommand>
{
    public CaptureDeliveryProofCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("ShipmentId is required");

        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("TrackingNumber is required");

        RuleFor(x => x.ReceivedBy)
            .NotEmpty().WithMessage("ReceivedBy is required")
            .MaximumLength(100).WithMessage("ReceivedBy cannot exceed 100 characters");

        RuleFor(x => x.DeliveryDate)
            .NotEmpty().WithMessage("DeliveryDate is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1)).WithMessage("DeliveryDate cannot be in the future");

        RuleFor(x => x.SignatureImageBase64)
            .NotEmpty().WithMessage("Signature image is required");

        RuleFor(x => x.PhotoProofBase64)
            .NotEmpty().WithMessage("Photo proof is required");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");

        RuleFor(x => x.DeliveredBy)
            .NotEmpty().WithMessage("DeliveredBy is required");
    }
}
