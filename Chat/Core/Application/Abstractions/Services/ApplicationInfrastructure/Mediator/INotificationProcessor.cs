using Domain.Abstractions;

namespace Application.Abstractions.Services.ApplicationInfrastructure.Mediator;

public interface INotificationProcessor<in TNotification> where TNotification : INotification
{
    public Task ProcessAsync(TNotification notification, CancellationToken cancellationToken = default);
}