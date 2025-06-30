using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Services.ApplicationInfrastructure.Mediator.PipelineBehaviors;

public class ValidationPipelineBehavior<TRequest>(IEnumerable<IValidator<TRequest>> _validators) : IPipelineBehavior<TRequest> where TRequest : IRequest
{
    public async Task<IOperationResult> HandleAsync(TRequest request, Func<Task<IOperationResult>> next, CancellationToken cancellationToken)
    {
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(validator => validator.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(failure => failure != null)
            .ToArray();

        if (failures.Length == 0)
        {
            return await next();
        }
        
        var result = ResultsHelper.BadRequest(failures.Select(failure => failure.ErrorMessage).Aggregate((er1, er2) => er1 + ", " + er2));
        
        return result;
    }
}