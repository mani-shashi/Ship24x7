using FluentValidation;
using Ship24X7.Shipment.Application.Commands;

namespace Ship24X7.Shipment.Application.Validators;

/// <summary>
/// Validator for CreateShipmentCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class CreateShipmentCommandValidator : AbstractValidator<CreateShipmentCommand>
{
    public CreateShipmentCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required");

        RuleFor(x => x.IdempotencyKey)
            .NotEmpty().WithMessage("Idempotency key is required");

        // Sender Address
        RuleFor(x => x.SenderContactName)
            .NotEmpty().WithMessage("Sender contact name is required");
        RuleFor(x => x.SenderContactPhone)
            .NotEmpty().WithMessage("Sender contact phone is required");
        RuleFor(x => x.SenderAddressLine1)
            .NotEmpty().WithMessage("Sender address is required");
        RuleFor(x => x.SenderCity)
            .NotEmpty().WithMessage("Sender city is required");
        RuleFor(x => x.SenderState)
            .NotEmpty().WithMessage("Sender state is required");
        RuleFor(x => x.SenderPostalCode)
            .NotEmpty().WithMessage("Sender postal code is required")
            .Matches(@"^\d{6}$").WithMessage("Invalid postal code format");
        RuleFor(x => x.SenderCountry)
            .NotEmpty().WithMessage("Sender country is required");

        // Receiver Address
        RuleFor(x => x.ReceiverContactName)
            .NotEmpty().WithMessage("Receiver contact name is required");
        RuleFor(x => x.ReceiverContactPhone)
            .NotEmpty().WithMessage("Receiver contact phone is required");
        RuleFor(x => x.ReceiverAddressLine1)
            .NotEmpty().WithMessage("Receiver address is required");
        RuleFor(x => x.ReceiverCity)
            .NotEmpty().WithMessage("Receiver city is required");
        RuleFor(x => x.ReceiverState)
            .NotEmpty().WithMessage("Receiver state is required");
        RuleFor(x => x.ReceiverPostalCode)
            .NotEmpty().WithMessage("Receiver postal code is required")
            .Matches(@"^\d{6}$").WithMessage("Invalid postal code format");
        RuleFor(x => x.ReceiverCountry)
            .NotEmpty().WithMessage("Receiver country is required");

        // Items
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description)
                .NotEmpty().WithMessage("Item description is required");
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");
            item.RuleFor(i => i.Weight)
                .InclusiveBetween(0.01m, 10000m).WithMessage("Weight must be between 0.01 kg and 10,000 kg");
            item.RuleFor(i => i.Length)
                .InclusiveBetween(1m, 500m).WithMessage("Length must be between 1 cm and 500 cm");
            item.RuleFor(i => i.Width)
                .InclusiveBetween(1m, 500m).WithMessage("Width must be between 1 cm and 500 cm");
            item.RuleFor(i => i.Height)
                .InclusiveBetween(1m, 500m).WithMessage("Height must be between 1 cm and 500 cm");
        });

        RuleFor(x => x.ServiceRateId)
            .NotEmpty().WithMessage("Service rate is required");
    }
}
