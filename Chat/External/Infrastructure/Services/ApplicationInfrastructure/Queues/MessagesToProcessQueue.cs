using Application.Abstractions.Services.Communications.Data;
using Application.Dtos.Messaging;
using GS.CommonLibrary.Services;

namespace Infrastructure.Services.ApplicationInfrastructure.Queues;

public class MessagesToProcessQueue(IQueue<MessageToProcess> queue) : IMessagesToProcessQueue
{
    public Task WriteAsync(MessageToProcess message, CancellationToken cancellationToken)
    {
        return queue.WriteAsync(message, "messages", "messages", "messages", true, cancellationToken: cancellationToken);
    }

    public async Task<MessageToProcess?> ReadAsync(CancellationToken cancellationToken)
    {
        var response = await queue.ReadAsync("messages", "messages", cancellationToken: cancellationToken);
        
        return response.isSuccess ? response.data : null;
    }
}