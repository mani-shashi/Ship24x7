using FluentValidation;
using Ship24X7.Payment.Application.Commands;

namespace Ship24X7.Payment.Application.Validators;

/// <summary>
/// Validator for VerifyPaymentCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class VerifyPaymentCommandValidator : AbstractValidator<VerifyPaymentCommand>
{
    public VerifyPaymentCommandValidator()
    {
        RuleFor(x => x.RazorpayOrderId)
            .NotEmpty().WithMessage("Razorpay order ID is required")
            .Must(x => x.StartsWith("order_")).WithMessage("Invalid Razorpay order ID format");

        RuleFor(x => x.RazorpayPaymentId)
            .NotEmpty().WithMessage("Razorpay payment ID is required")
            .Must(x => x.StartsWith("pay_")).WithMessage("Invalid Razorpay payment ID format");

        RuleFor(x => x.RazorpaySignature)
            .NotEmpty().WithMessage("Razorpay signature is required");
    }
}
