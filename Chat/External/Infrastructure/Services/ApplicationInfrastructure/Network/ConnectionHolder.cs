using System.Buffers;
using System.Net;
using System.Net.WebSockets;
using Application.Abstractions.Services.Communications;
using Application.Abstractions.Services.Communications.Data;
using Application.Abstractions.Services.Net;
using Application.Common.Mappers;
using Application.Messaging.Proto.Messages;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.ApplicationInfrastructure.Network;

public class ConnectionHolder(HttpListener listener, IWebSocketStore webSocketStore, IRawMessagesQueue messagesQueue, ILogger<ConnectionHolder> logger) : IConnectionHolder
{
    private bool _isStopRequested;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        listener.Start();

        while (cancellationToken.IsCancellationRequested is false && _isStopRequested is false)
        {
            var context = await listener.GetContextAsync();

            if (context.Request.IsWebSocketRequest)
            {
                var ConnectionHolderHandler = new ConnectionHolderHandler(context, webSocketStore, messagesQueue, logger);
                ThreadPool.UnsafeQueueUserWorkItem(ConnectionHolderHandler, false);
            }
            else
            {
                context.Response.StatusCode = 418;
                context.Response.Close();
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _isStopRequested = true;
        listener.Stop();
        return Task.CompletedTask;
    }
}

file class ConnectionHolderHandler(HttpListenerContext context, IWebSocketStore webSocketStore, IRawMessagesQueue messagesQueue, ILogger<ConnectionHolder> logger) : IThreadPoolWorkItem
{
    // ReSharper disable once AsyncVoidMethod
    public async void Execute()
    {
        HttpListenerWebSocketContext wsContext = null!;
        var id = Guid.Empty;
        var buffer = ArrayPool<byte>.Shared.Rent(512000);
        try
        {
            wsContext = await context.AcceptWebSocketAsync(null);
            id = webSocketStore.AddSocket(wsContext.WebSocket);
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(1000 * 60 * 6);
            var result = await wsContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);

            using var stream = new MemoryStream(buffer, 0, result.Count);
            var connRequest = ConnectionRequest.Parser.ParseFrom(stream);

            if (connRequest is null)
            {
                logger.LogError("Received invalid message {Headers}, {Uri}", wsContext.Headers, wsContext.RequestUri);
                return;
            }
            
            connRequest.OptionalConnectionId = id.ToString();

            await messagesQueue.WriteAsync(connRequest.ToRawMessage(), cancellationToken: CancellationToken.None);

            while (wsContext.WebSocket.State == WebSocketState.Open && cancellationTokenSource.IsCancellationRequested is false)
            {
                cancellationTokenSource.CancelAfter(1000 * 60 * 6);
                result = await wsContext.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                using var msgStream = new MemoryStream(buffer, 0, result.Count);
                var received = ReceivedMessage.Parser.WithDiscardUnknownFields(true).ParseFrom(msgStream);
                
                if (received is null)
                {
                    logger.LogError("Received invalid message {Headers}, {Uri}", wsContext.Headers, wsContext.RequestUri);
                    continue;
                }

                received.OptionalConnectionId = id.ToString();
                await messagesQueue.WriteAsync(received.ToRawMessage(), cancellationToken: CancellationToken.None);
            }
        }
        catch (Exception e)
        {
            if (e is not OperationCanceledException)
            {
                logger.LogError(e, "Error in ConnectionHolderHandler");
            }
            else
            {
                logger.LogInformation(e, "Connection closed automatically by timeout");
            }

            await webSocketStore.RemoveSocketAsync(id, "error while handling connection");
        }
        finally
        {
            wsContext.WebSocket.Dispose();
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}