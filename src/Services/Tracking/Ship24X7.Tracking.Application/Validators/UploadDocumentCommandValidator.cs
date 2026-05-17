using FluentValidation;
using Ship24X7.Tracking.Application.Commands;

namespace Ship24X7.Tracking.Application.Validators;

/// <summary>
/// Validator for UploadDocumentCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class UploadDocumentCommandValidator : AbstractValidator<UploadDocumentCommand>
{
    private static readonly string[] AllowedContentTypes = { "application/pdf", "image/jpeg", "image/jpg", "image/png" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public UploadDocumentCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("ShipmentId is required");

        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("TrackingNumber is required");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("FileName is required")
            .MaximumLength(255).WithMessage("FileName cannot exceed 255 characters");

        RuleFor(x => x.FileContent)
            .NotEmpty().WithMessage("FileContent is required")
            .Must(content => content.Length <= MaxFileSizeBytes)
            .WithMessage($"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("ContentType is required")
            .Must(ct => AllowedContentTypes.Contains(ct.ToLower()))
            .WithMessage("File type must be PDF, JPG, or PNG");

        RuleFor(x => x.UploadedBy)
            .NotEmpty().WithMessage("UploadedBy is required");
    }
}
