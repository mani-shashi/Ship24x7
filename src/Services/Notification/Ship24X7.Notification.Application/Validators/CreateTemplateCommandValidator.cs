using FluentValidation;
using Ship24X7.Notification.Application.Commands;

namespace Ship24X7.Notification.Application.Validators;

/// <summary>
/// Validator for CreateTemplateCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Subject is required")
            .MaximumLength(500).WithMessage("Subject must not exceed 500 characters");

        RuleFor(x => x.BodyTemplate)
            .NotEmpty().WithMessage("BodyTemplate is required");

        RuleFor(x => x.RequiredPlaceholders)
            .NotNull().WithMessage("RequiredPlaceholders is required");

        RuleFor(x => x.CreatedBy)
            .NotEmpty().WithMessage("CreatedBy is required");
    }
}
