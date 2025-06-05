using Application.Abstractions.Services.ApplicationInfrastructure.Results;

namespace Application.Abstractions.Services.ApplicationInfrastructure.Mediator;

public interface IRequestHandler<in TRequest> where TRequest : IRequest
{
    public Task<IOperationResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
}