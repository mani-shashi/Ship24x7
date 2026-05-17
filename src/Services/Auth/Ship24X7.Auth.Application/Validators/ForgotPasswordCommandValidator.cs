using FluentValidation;
using Ship24X7.Auth.Application.Commands;

namespace Ship24X7.Auth.Application.Validators;

/// <summary>
/// Validator for forgot password command ensuring email is provided and valid.
/// </summary>
public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the ForgotPasswordCommandValidator class with validation rules.
    /// </summary>
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");
    }
}
