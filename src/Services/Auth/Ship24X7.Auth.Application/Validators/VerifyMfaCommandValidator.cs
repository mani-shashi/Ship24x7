using FluentValidation;
using Ship24X7.Auth.Application.Commands;

namespace Ship24X7.Auth.Application.Validators;

/// <summary>
/// Validator for verify MFA command ensuring user ID and valid MFA code are provided.
/// MFA code must be exactly 6 digits.
/// </summary>
public class VerifyMfaCommandValidator : AbstractValidator<VerifyMfaCommand>
{
    /// <summary>
    /// Initializes a new instance of the VerifyMfaCommandValidator class with validation rules.
    /// </summary>
    public VerifyMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("MFA code is required")
            .Length(6).WithMessage("MFA code must be 6 digits");
    }
}
