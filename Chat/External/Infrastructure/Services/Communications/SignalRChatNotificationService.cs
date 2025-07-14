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

    public async Task NotifyNewMessageWithAttachmentAsync(Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("ReceiveMessage", new
            {
                ChatId = chatId,
                SenderId = senderId,
                SenderName = senderName,
                Content = message,
                AttachmentUrl = attachmentUrl,
                SentAt = sentAt
            });
    }

    public async Task NotifyUserAboutNewMessageAsync(Guid userId, Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt, string? chatName = null)
    {
        await hubContext.Clients.Group($"User_{userId}")
            .SendAsync("NewMessageNotification", new
            {
                ChatId = chatId,
                ChatName = chatName,
                SenderId = senderId,
                SenderName = senderName,
                Content = message,
                SentAt = sentAt,
                HasAttachment = false
            });
    }

    public async Task NotifyUserAboutNewMessageWithAttachmentAsync(Guid userId, Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt, string? chatName = null)
    {
        await hubContext.Clients.Group($"User_{userId}")
            .SendAsync("NewMessageNotification", new
            {
                ChatId = chatId,
                ChatName = chatName,
                SenderId = senderId,
                SenderName = senderName,
                Content = message,
                AttachmentUrl = attachmentUrl,
                SentAt = sentAt,
                HasAttachment = true
            });
    }

    public async Task NotifyUsersAboutNewMessageAsync(IEnumerable<Guid> userIds, Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt, string? chatName = null)
    {
        var tasks = userIds.Select(userId => 
            NotifyUserAboutNewMessageAsync(userId, chatId, senderId, senderName, message, sentAt, chatName));
        
        await Task.WhenAll(tasks);
    }

    public async Task NotifyUsersAboutNewMessageWithAttachmentAsync(IEnumerable<Guid> userIds, Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt, string? chatName = null)
    {
        var tasks = userIds.Select(userId => 
            NotifyUserAboutNewMessageWithAttachmentAsync(userId, chatId, senderId, senderName, message, attachmentUrl, sentAt, chatName));
        
        await Task.WhenAll(tasks);
    }

    public async Task NotifyNewMessageToAllChatParticipantsAsync(Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt, Guid messageId, string? chatName = null)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("ReceiveMessage", new
            {
                MessageId = messageId,
                ChatId = chatId,
                SenderId = senderId,
                SenderName = senderName,
                Content = message,
                SentAt = sentAt
            });

        await hubContext.Clients.Group($"ChatParticipants_{chatId}")
            .SendAsync("NewMessageNotification", new
            {
                MessageId = messageId,
                ChatId = chatId,
                ChatName = chatName,
                SenderId = senderId,
                SenderName = senderName,
                Content = message,
                SentAt = sentAt,
                HasAttachment = false
            });
    }

    public async Task NotifyNewMessageWithAttachmentToAllChatParticipantsAsync(Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt, Guid messageId, string? chatName = null)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("ReceiveMessage", new
            {
                MessageId = messageId,
                ChatId = chatId,
                SenderId = senderId,
                SenderName = senderName,
                Content = message,
                AttachmentUrl = attachmentUrl,
                SentAt = sentAt
            });

        await hubContext.Clients.Group($"ChatParticipants_{chatId}")
            .SendAsync("NewMessageNotification", new
            {
                MessageId = messageId,
                ChatId = chatId,
                ChatName = chatName,
                SenderId = senderId,
                SenderName = senderName,
                Content = message,
                AttachmentUrl = attachmentUrl,
                SentAt = sentAt,
                HasAttachment = true
            });
    }

    public async Task NotifyUserJoinedChatAsync(Guid chatId, Guid userId, string username, string? chatName = null)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("UserJoinedChat", new
            {
                ChatId = chatId,
                ChatName = chatName,
                UserId = userId,
                Username = username,
                JoinedAt = DateTime.UtcNow
            });

        await hubContext.Clients.Group($"ChatParticipants_{chatId}")
            .SendAsync("ChatEvent", new
            {
                EventType = "UserJoined",
                ChatId = chatId,
                ChatName = chatName,
                UserId = userId,
                Username = username,
                Timestamp = DateTime.UtcNow
            });
    }

    public async Task NotifyUserLeftChatAsync(Guid chatId, Guid userId, string username, string? chatName = null)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("UserLeftChat", new
            {
                ChatId = chatId,
                ChatName = chatName,
                UserId = userId,
                Username = username,
                LeftAt = DateTime.UtcNow
            });

        await hubContext.Clients.Group($"ChatParticipants_{chatId}")
            .SendAsync("ChatEvent", new
            {
                EventType = "UserLeft",
                ChatId = chatId,
                ChatName = chatName,
                UserId = userId,
                Username = username,
                Timestamp = DateTime.UtcNow
            });
    }

    public async Task NotifyUserTypingAsync(Guid chatId, Guid userId, string username, bool isTyping)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("UserTyping", new
            {
                ChatId = chatId,
                UserId = userId,
                Username = username,
                IsTyping = isTyping,
                Timestamp = DateTime.UtcNow
            });
    }

    public async Task NotifyMessageReadAsync(Guid chatId, Guid userId, Guid messageId)
    {
        await hubContext.Clients.Group($"Chat_{chatId}")
            .SendAsync("MessageRead", new
            {
                ChatId = chatId,
                UserId = userId,
                MessageId = messageId,
                ReadAt = DateTime.UtcNow
            });
    }
} 