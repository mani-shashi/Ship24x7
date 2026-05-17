using FluentValidation;
using Ship24X7.Notification.Application.Commands;

namespace Ship24X7.Notification.Application.Validators;

/// <summary>
/// Validator for UpdatePreferenceCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class UpdatePreferenceCommandValidator : AbstractValidator<UpdatePreferenceCommand>
{
    public UpdatePreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}
