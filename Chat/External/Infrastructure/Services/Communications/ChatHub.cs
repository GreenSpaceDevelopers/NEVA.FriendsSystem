using Microsoft.AspNetCore.SignalR;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Infrastructure.Services.Communications;

[Authorize]
public class ChatHub(ISender sender, ILogger<ChatHub> logger) : Hub
{
    private Guid GetCurrentUserId()
    {
        var http = Context.GetHttpContext();
        if (http is null) return Guid.Empty;

        if (http.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authVal = authHeader.ToString();
            if (!string.IsNullOrWhiteSpace(authVal) && authVal.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authVal[7..].Trim();
                if (Guid.TryParse(token, out var guidHdr)) return guidHdr;
            }
        }

        if (http.Request.Query.TryGetValue("access_token", out var tokenVals))
        {
            var token = tokenVals.FirstOrDefault();
            if (!string.IsNullOrEmpty(token) && Guid.TryParse(token, out var guidQry)) return guidQry;
        }

        return Guid.Empty;
    }

    public async Task JoinChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
        
        await Clients.Group($"Chat_{chatId}").SendAsync("UserJoined", new
        {
            ConnectionId = Context.ConnectionId,
            ChatId = chatId,
            JoinedAt = DateTime.UtcNow
        });
    }

    public async Task LeaveChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
        
        await Clients.Group($"Chat_{chatId}").SendAsync("UserLeft", new
        {
            ConnectionId = Context.ConnectionId,
            ChatId = chatId,
            LeftAt = DateTime.UtcNow
        });
    }

    public async Task SendMessage(string chatId, string message)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Invalid user token"); return;
        }

        var command = new SendMessageCommand(Guid.Parse(chatId), userId, message);
        var result = await sender.SendAsync(command);

        if (result.IsSuccess)
        {
            await Clients.Group($"Chat_{chatId}").SendAsync("ReceiveMessage", new
            {
                MessageId = result.ObjectData,
                ChatId = chatId,
                SenderId = userId,
                Content = message,
                SentAt = DateTime.UtcNow,
                SentViaConnectionId = Context.ConnectionId
            });
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) { await Clients.Caller.SendAsync("Error", "Invalid user token"); return; }
        await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task GetGroupInfo(string chatId)
    {
        await Clients.Caller.SendAsync("GroupInfo", new
        {
            ChatId = chatId,
            YourConnectionId = Context.ConnectionId,
            Message = $"Вы подключены к группе Chat_{chatId}. Ваш ConnectionId: {Context.ConnectionId}",
            Timestamp = DateTime.UtcNow
        });
    }
} 