using FluentValidation;
using Ship24X7.Payment.Application.Commands;

namespace Ship24X7.Payment.Application.Validators;

/// <summary>
/// Validator for CreatePaymentOrderCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class CreatePaymentOrderCommandValidator : AbstractValidator<CreatePaymentOrderCommand>
{
    public CreatePaymentOrderCommandValidator()
    {
        RuleFor(x => x.ShipmentId)
            .NotEmpty().WithMessage("Shipment ID is required");

        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("Tracking number is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter code");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Idempotency key is required");
    }
}
