using FluentValidation;
using Ship24X7.Shipment.Application.Queries;

namespace Ship24X7.Shipment.Application.Validators;

/// <summary>
/// Query for retrieving calculateratevalidator data. Defines query parameters and result type.
/// </summary>
public class CalculateRateQueryValidator : AbstractValidator<CalculateRateQuery>
{
    public CalculateRateQueryValidator()
    {
        RuleFor(x => x.ActualWeight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0");

        RuleFor(x => x.Length)
            .InclusiveBetween(1m, 500m).WithMessage("Length must be between 1 cm and 500 cm");

        RuleFor(x => x.Width)
            .InclusiveBetween(1m, 500m).WithMessage("Width must be between 1 cm and 500 cm");

        RuleFor(x => x.Height)
            .InclusiveBetween(1m, 500m).WithMessage("Height must be between 1 cm and 500 cm");
    }
}
