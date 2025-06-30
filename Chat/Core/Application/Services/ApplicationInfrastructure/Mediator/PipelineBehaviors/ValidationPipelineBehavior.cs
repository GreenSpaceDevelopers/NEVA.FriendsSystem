using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Services.ApplicationInfrastructure.Mediator.PipelineBehaviors;

public class ValidationPipelineBehavior<TRequest>(IValidator<TRequest> validator) : IPipelineBehavior<TRequest> where TRequest : IRequest
{
    public async Task<IOperationResult> HandleAsync(TRequest request, Func<Task<IOperationResult>> next, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return ResultsHelper.BadRequest(validationResult.Errors.Select(e => e.ErrorMessage).Aggregate((e1, e2) => $"{e1}, {e2}"));
        }
        
        return await next();
    }
}