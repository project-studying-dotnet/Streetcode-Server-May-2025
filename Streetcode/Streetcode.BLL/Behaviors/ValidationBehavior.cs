using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Streetcode.BLL.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = await ValidateAsync(request, cancellationToken);

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }

        var response = await next();

        return response;
    }

    private async Task<List<ValidationFailure>> ValidateAsync(
        TRequest request,
        CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var tasks = _validators.Select(v => v.ValidateAsync(context, cancellationToken));
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);

        var failures = new List<ValidationFailure>();

        var allFailures = results
            .Where(r => r != null)
            .SelectMany(r => r.Errors)
            .Where(e => e != null);
        failures.AddRange(allFailures);

        return failures;
    }
}