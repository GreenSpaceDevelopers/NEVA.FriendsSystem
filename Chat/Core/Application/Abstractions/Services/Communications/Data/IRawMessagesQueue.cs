using Application.Dtos.Messaging;

namespace Application.Abstractions.Services.Communications.Data;

public interface IRawMessagesQueue
{
    public Task WriteAsync(RawMessage message, CancellationToken cancellationToken);
    public Task<RawMessage> ReadAsync(CancellationToken cancellationToken);
}