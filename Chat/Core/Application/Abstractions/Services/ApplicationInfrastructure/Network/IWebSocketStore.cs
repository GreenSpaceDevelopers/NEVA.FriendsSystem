using System.Net.WebSockets;

namespace Application.Abstractions.Services.Net;

public interface IWebSocketStore
{
    public Guid AddSocket(WebSocket socket);
    public WebSocket? GetSocketById(Guid id);

    public ValueTask RemoveSocketAsync(Guid id, string reason = "Closed by server", WebSocketCloseStatus reasonCode = WebSocketCloseStatus.InternalServerError,
        CancellationToken cancellationToken = default);
}