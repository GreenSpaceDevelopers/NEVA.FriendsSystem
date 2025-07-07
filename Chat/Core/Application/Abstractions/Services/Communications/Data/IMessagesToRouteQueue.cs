using Application.Dtos.Messaging;

namespace Application.Abstractions.Services.Communications.Data;

public interface IMessagesToRouteQueue
{
    public Task WriteAsync(MessageToRoute message, CancellationToken cancellationToken);
    public Task<MessageToRoute?> ReadAsync(CancellationToken cancellationToken);
}