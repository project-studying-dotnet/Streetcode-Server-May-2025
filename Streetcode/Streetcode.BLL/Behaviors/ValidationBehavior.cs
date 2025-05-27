using System.Reflection;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using MediatR;

namespace Streetcode.BLL.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IReadOnlyCollection<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators as IReadOnlyCollection<IValidator<TRequest>>
                      ?? validators.ToArray();
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Count == 0)
        {
            return await next().ConfigureAwait(false);
        }

        var failures = await ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (failures.Count == 0)
        {
            return await next().ConfigureAwait(false);
        }

        return BuildFailureResult(failures);
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

    private static TResponse BuildFailureResult(IReadOnlyCollection<ValidationFailure> failures)
    {
        var errors = failures.Select(f => new Error(f.ErrorMessage));

        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(errors);
        }

        if (typeof(TResponse).IsGenericType
            && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            var payloadType = typeof(TResponse).GetGenericArguments()[0];

            var failMethod = typeof(Result)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(m => m is { Name: nameof(Result.Fail), IsGenericMethodDefinition: true }
                            && m.GetParameters().Length == 1);

            var genericFail = failMethod.MakeGenericMethod(payloadType);
            var instance = genericFail.Invoke(null, new object?[] { errors });

            return (TResponse)instance!;
        }

        throw new ValidationException(failures);
    }
}
