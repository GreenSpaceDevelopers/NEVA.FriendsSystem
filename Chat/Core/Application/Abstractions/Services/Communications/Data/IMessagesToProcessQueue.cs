using Application.Dtos.Messaging;

namespace Application.Abstractions.Services.Communications.Data;

public interface IMessagesToProcessQueue
{
    public Task WriteAsync(MessageToProcess message, CancellationToken cancellationToken);
    public Task<MessageToProcess?> ReadAsync(CancellationToken cancellationToken);
}