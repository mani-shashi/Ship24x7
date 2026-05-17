using FluentValidation;
using Ship24X7.Auth.Application.Commands;

namespace Ship24X7.Auth.Application.Validators;

/// <summary>
/// Validator for disable MFA command ensuring user ID, password, and valid MFA code are provided.
/// MFA code must be exactly 6 digits.
/// </summary>
public class DisableMfaCommandValidator : AbstractValidator<DisableMfaCommand>
{
    /// <summary>
    /// Initializes a new instance of the DisableMfaCommandValidator class with validation rules.
    /// </summary>
    public DisableMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");

        RuleFor(x => x.MfaCode)
            .NotEmpty().WithMessage("MFA code is required")
            .Length(6).WithMessage("MFA code must be 6 digits");
    }
}
