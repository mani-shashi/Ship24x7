using FluentValidation;
using Ship24X7.Tracking.Application.Commands;

namespace Ship24X7.Tracking.Application.Validators;

/// <summary>
/// Validator for RecordTrackingEventCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class RecordTrackingEventCommandValidator : AbstractValidator<RecordTrackingEventCommand>
{
    public RecordTrackingEventCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("ShipmentId is required");

        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("TrackingNumber is required")
            .MaximumLength(50).WithMessage("TrackingNumber cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location cannot exceed 200 characters");

        RuleFor(x => x.EventTimestamp)
            .NotEmpty().WithMessage("EventTimestamp is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddHours(1)).WithMessage("EventTimestamp cannot be in the future");

        RuleFor(x => x.ExceptionReason)
            .MaximumLength(500).WithMessage("ExceptionReason cannot exceed 500 characters")
            .When(x => x.IsException);
    }
}
