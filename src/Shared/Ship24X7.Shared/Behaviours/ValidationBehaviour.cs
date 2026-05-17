using FluentValidation;
using MediatR;

namespace Ship24X7.Shared.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that runs all registered FluentValidation validators
/// before the command/query handler executes.
///
/// If any validator fails, throws <see cref="ValidationException"/> with all
/// failure messages collected. The GlobalExceptionMiddleware maps this to 400.
///
/// This keeps handlers free of manual validation checks and ensures every
/// command/query is validated consistently before any business logic runs.
/// </summary>
public sealed class ValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = (await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
