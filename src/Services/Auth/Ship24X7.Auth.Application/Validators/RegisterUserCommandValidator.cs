using FluentValidation;
using Ship24X7.Auth.Application.Commands;
using System.Text.RegularExpressions;

namespace Ship24X7.Auth.Application.Validators;

/// <summary>
/// Validator for user registration command ensuring all required fields meet security requirements.
/// Enforces strong password policy with minimum length and complexity requirements.
/// </summary>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    private static readonly Regex PasswordRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Initializes a new instance of the RegisterUserCommandValidator class with validation rules.
    /// Password must contain at least 8 characters with uppercase, lowercase, digit, and special character.
    /// </summary>
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(PasswordRegex).WithMessage(
                "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required");
    }
}
