using Application.Abstractions.Services.Communications.Data;
using Application.Dtos.Messaging;
using GS.CommonLibrary.Services;

namespace Infrastructure.Services.ApplicationInfrastructure.Queues;

public class MessagesToRouteQueue(IQueue<MessageToRoute> queue) : IMessagesToRouteQueue
{
    public Task WriteAsync(MessageToRoute message, CancellationToken cancellationToken)
    {
        return queue.WriteAsync(message, "route", "route", "route", true, cancellationToken: cancellationToken);
    }

    public async Task<MessageToRoute?> ReadAsync(CancellationToken cancellationToken)
    {
        var response = await queue.ReadAsync("route", "route", cancellationToken: cancellationToken);
        
        return response.isSuccess ? response.data : null;
    }
}