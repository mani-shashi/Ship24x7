using FluentValidation;
using Ship24X7.Auth.Application.Commands;

namespace Ship24X7.Auth.Application.Validators;

/// <summary>
/// Validator for enable MFA command ensuring user ID is provided.
/// </summary>
public class EnableMfaCommandValidator : AbstractValidator<EnableMfaCommand>
{
    /// <summary>
    /// Initializes a new instance of the EnableMfaCommandValidator class with validation rules.
    /// </summary>
    public EnableMfaCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
