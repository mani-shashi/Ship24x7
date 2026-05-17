using FluentValidation;
using Ship24X7.Auth.Application.Commands;

namespace Ship24X7.Auth.Application.Validators;

/// <summary>
/// Validator for email verification command ensuring token is provided and valid format.
/// </summary>
public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    /// <summary>
    /// Initializes a new instance of the VerifyEmailCommandValidator class with validation rules.
    /// </summary>
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Verification token is required")
            .Length(32).WithMessage("Invalid verification token format");
    }
}
