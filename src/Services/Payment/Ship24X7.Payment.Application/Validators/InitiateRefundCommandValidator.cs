using FluentValidation;
using Ship24X7.Payment.Application.Commands;

namespace Ship24X7.Payment.Application.Validators;

/// <summary>
/// Validator for InitiateRefundCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class InitiateRefundCommandValidator : AbstractValidator<InitiateRefundCommand>
{
    public InitiateRefundCommandValidator()
    {
        RuleFor(x => x.PaymentOrderId)
            .NotEmpty().WithMessage("Payment order ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than zero");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Refund reason is required")
            .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters");

        RuleFor(x => x.InitiatedBy)
            .NotEmpty().WithMessage("Initiated by user ID is required");
    }
}
