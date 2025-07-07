using Application.Abstractions.Services.Communications;
using Microsoft.AspNetCore.SignalR;

namespace Infrastructure.Services.Communications;

public class SignalRChatNotificationService(IHubContext<ChatHub> hubContext) : IChatNotificationService
{
    public async Task NotifyNewMessageAsync(Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("ReceiveMessage", new
            {
                ChatId = chatId,
                SenderId = senderId,
                SenderName = senderName,
                Content = message,
                SentAt = sentAt
            });
    }

    public async Task NotifyUserJoinedChatAsync(Guid chatId, Guid userId, string userName)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("UserJoined", new
            {
                ChatId = chatId,
                UserId = userId,
                UserName = userName
            });
    }

    public async Task NotifyUserLeftChatAsync(Guid chatId, Guid userId, string userName)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("UserLeft", new
            {
                ChatId = chatId,
                UserId = userId,
                UserName = userName
            });
    }
} 