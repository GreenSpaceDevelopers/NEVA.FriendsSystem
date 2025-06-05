using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services.ApplicationInfrastructure.Mediator;

public class Sender(IServiceProvider serviceProvider) : ISender
{
    public Task<IOperationResult> SendAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        using var scope = serviceProvider.CreateScope();
        var handler = scope.ServiceProvider.GetService<IRequestHandler<TRequest>>();

        if (handler is null)
        {
            throw new NotImplementedException($"Handler for request {request.GetType()} not found");
        }

        var behaviors = serviceProvider
            .GetServices<IPipelineBehavior<TRequest>>()
            .Reverse()
            .ToList();

        var handlerDelegate = () => handler.HandleAsync(request, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.HandleAsync(request, next, cancellationToken);
        }

        return handlerDelegate();
    }

    public Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        using var scope = serviceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetService<INotificationProcessor<TNotification>>();

        if (processor is null)
        {
            throw new NotImplementedException($"Processor for notification {notification.GetType()} not found");
        }

        var behaviors = serviceProvider
            .GetServices<INotificationBehavior<TNotification>>()
            .Reverse()
            .ToList();

        var handlerDelegate = () => processor.ProcessAsync(notification, cancellationToken);

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.HandleAsync(notification, next, cancellationToken);
        }

        return handlerDelegate();
    }
}