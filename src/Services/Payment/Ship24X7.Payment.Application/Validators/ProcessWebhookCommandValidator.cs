using FluentValidation;
using Ship24X7.Payment.Application.Commands;

namespace Ship24X7.Payment.Application.Validators;

/// <summary>
/// Validator for ProcessWebhookCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class ProcessWebhookCommandValidator : AbstractValidator<ProcessWebhookCommand>
{
    public ProcessWebhookCommandValidator()
    {
        RuleFor(x => x.EventType)
            .NotEmpty().WithMessage("Event type is required");

        RuleFor(x => x.Payload)
            .NotEmpty().WithMessage("Payload is required");

        RuleFor(x => x.Signature)
            .NotEmpty().WithMessage("Signature is required");
    }
}
