using GestorInventario.Application.Common.Validation;
using GestorInventario.Application.Exceptions;
using MediatR;

namespace GestorInventario.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IRequestValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IRequestValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var errors = _validators
            .SelectMany(validator => validator.Validate(request))
            .GroupBy(error => error.Key, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .SelectMany(error => error.Value)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray(),
                StringComparer.Ordinal);

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        return await next();
    }
}
