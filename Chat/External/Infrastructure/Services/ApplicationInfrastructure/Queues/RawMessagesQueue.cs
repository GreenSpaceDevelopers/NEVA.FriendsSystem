using Application.Abstractions.Services.Communications.Data;
using Application.Dtos.Messaging;
using GS.CommonLibrary.Services;

namespace Infrastructure.Services.ApplicationInfrastructure.Queues;

public class RawMessagesQueue(IQueue<RawMessage> queue) : IRawMessagesQueue
{
    public Task WriteAsync(RawMessage message, CancellationToken cancellationToken)
    {
        return queue.WriteAsync(message, "raw", "raw", "raw", true, cancellationToken: cancellationToken);
    }

    public async Task<RawMessage?> ReadAsync(CancellationToken cancellationToken)
    {
        var response = await queue.ReadAsync("raw", "raw", cancellationToken: cancellationToken);
        
        return response.isSuccess ? response.data : null;
    }
}