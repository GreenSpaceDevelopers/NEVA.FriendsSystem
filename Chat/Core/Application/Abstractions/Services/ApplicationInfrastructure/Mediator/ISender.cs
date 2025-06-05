using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Abstractions;

namespace Application.Abstractions.Services.ApplicationInfrastructure.Mediator;

public interface ISender
{
    public Task<IOperationResult> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest;
    public Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification;
}