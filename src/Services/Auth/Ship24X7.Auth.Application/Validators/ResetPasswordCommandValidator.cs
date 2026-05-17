using FluentValidation;
using Ship24X7.Auth.Application.Commands;

namespace Ship24X7.Auth.Application.Validators;

/// <summary>
/// Validator for reset password command ensuring token and password meet requirements.
/// </summary>
public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    /// <summary>
    /// Initializes a new instance of the ResetPasswordCommandValidator class with validation rules.
    /// </summary>
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required")
            .Length(32).WithMessage("Invalid reset token format");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"\d").WithMessage("Password must contain at least one digit")
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }
}
