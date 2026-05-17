using FluentValidation;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Domain.ValueObjects;

namespace Ship24X7.Notification.Application.Validators;

/// <summary>
/// Validator for SendNotificationCommand ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("TemplateId is required");

        RuleFor(x => x.RecipientEmail)
            .NotEmpty().WithMessage("RecipientEmail is required")
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => x.Channel == NotificationChannel.Email);

        RuleFor(x => x.RecipientPhone)
            .NotEmpty().WithMessage("RecipientPhone is required")
            .When(x => x.Channel == NotificationChannel.SMS);

        RuleFor(x => x.PlaceholderData)
            .NotNull().WithMessage("PlaceholderData is required");
    }
}
