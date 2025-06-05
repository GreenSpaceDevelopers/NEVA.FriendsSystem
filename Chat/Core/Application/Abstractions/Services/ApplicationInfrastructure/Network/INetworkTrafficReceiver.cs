using Application.Dtos.Messaging;

namespace Application.Abstractions.Services.Net;

public interface INetworkTrafficReceiver
{
    public Task<RawMessage> ReadAsync(CancellationToken cancellationToken);
}