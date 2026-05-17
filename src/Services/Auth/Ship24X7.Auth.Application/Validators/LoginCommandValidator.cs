using FluentValidation;
using Ship24X7.Auth.Application.Commands;

namespace Ship24X7.Auth.Application.Validators;

/// <summary>
/// Validator for login command ensuring email and password are provided.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// Initializes a new instance of the LoginCommandValidator class with validation rules.
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
