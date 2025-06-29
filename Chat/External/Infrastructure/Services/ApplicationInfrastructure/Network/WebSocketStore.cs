using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using Application.Abstractions.Services.Net;

namespace Infrastructure.Services.ApplicationInfrastructure.Network;

public class WebSocketStore : IWebSocketStore
{
    private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();

    public Guid AddSocket(WebSocket socket)
    {
        var connectionId = Guid.NewGuid();
        _sockets.TryAdd(connectionId, socket);
        return connectionId;
    }

    public WebSocket? GetSocketById(Guid id)
    {
        _sockets.TryGetValue(id, out var socket);

        return socket;
    }

    [AsyncMethodBuilder(typeof(PoolingAsyncValueTaskMethodBuilder))]
    public async ValueTask RemoveSocketAsync(Guid id, string reason = "Closed by server",
        WebSocketCloseStatus reasonCode = WebSocketCloseStatus.InternalServerError, CancellationToken cancellationToken = default)
    {
        _sockets.TryRemove(id, out var socket);

        if (socket != null)
        {
            await socket.CloseAsync(reasonCode, reason, cancellationToken);
        }
    }
}