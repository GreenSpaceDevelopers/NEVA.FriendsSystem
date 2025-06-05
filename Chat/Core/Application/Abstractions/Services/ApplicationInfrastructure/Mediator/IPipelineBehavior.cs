using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Abstractions;

namespace Application.Abstractions.Services.ApplicationInfrastructure.Mediator;

public interface IPipelineBehavior<in TRequest> where TRequest : IRequest
{
    public Task<IOperationResult> HandleAsync(TRequest request, Func<Task<IOperationResult>> next, CancellationToken cancellationToken);
}

public interface INotificationBehavior<in TNotification> where TNotification : INotification
{
    public Task<IOperationResult> HandleAsync(TNotification notification, Func<Task> next, CancellationToken cancellationToken);
}