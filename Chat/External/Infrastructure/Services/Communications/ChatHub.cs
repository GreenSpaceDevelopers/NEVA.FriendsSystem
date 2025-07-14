using Microsoft.AspNetCore.SignalR;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Requests.Commands.Messaging;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.Communications;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Communications;

public class ChatHub(ISender sender, IChatsRepository chatsRepository, IChatUsersRepository chatUsersRepository, IChatNotificationService notificationService, ILogger<ChatHub> logger) : Hub
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

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) 
        { 
            await Clients.Caller.SendAsync("Error", "Invalid user token"); 
            return; 
        }

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            
            var userChatIds = await chatsRepository.GetUserChatIdsAsync(userId);
            
            var joinTasks = new List<Task>();
            foreach (var chatId in userChatIds)
            {
                joinTasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}"));
                joinTasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, $"ChatParticipants_{chatId}"));
            }
            
            await Task.WhenAll(joinTasks);
            
            logger.LogInformation("User {UserId} connected with {ConnectionId} and joined {ChatCount} chats", 
                userId, Context.ConnectionId, userChatIds.Count);

            await Clients.Caller.SendAsync("Connected", new
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow,
                JoinedChatsCount = userChatIds.Count,
                JoinedChatIds = userChatIds
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error connecting user {UserId}", userId);
            await Clients.Caller.SendAsync("Error", $"Connection failed: {ex.Message}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            if (userId != Guid.Empty)
            {
                var userChatIds = await chatsRepository.GetUserChatIdsAsync(userId);
                
                var leaveTasks = new List<Task>();
                foreach (var chatId in userChatIds)
                {
                    leaveTasks.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}"));
                    leaveTasks.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ChatParticipants_{chatId}"));
                }
                
                leaveTasks.Add(Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}"));
                
                await Task.WhenAll(leaveTasks);
                
                logger.LogInformation("User {UserId} disconnected from {ConnectionId} and left {ChatCount} chats", 
                    userId, Context.ConnectionId, userChatIds.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disconnecting user {UserId}", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(string chatId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Invalid user token");
            return;
        }

        if (!Guid.TryParse(chatId, out var chatGuid))
        {
            await Clients.Caller.SendAsync("Error", "Invalid chat ID");
            return;
        }

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
            await Groups.AddToGroupAsync(Context.ConnectionId, $"ChatParticipants_{chatId}");
            
            await Clients.Caller.SendAsync("JoinedChat", new
            {
                ChatId = chatGuid,
                JoinedAt = DateTime.UtcNow
            });
            
            var user = await chatUsersRepository.GetByIdAsync(userId);
            var chat = await chatsRepository.GetByIdAsync(chatGuid);
            
            if (user != null && chat != null)
            {
                await notificationService.NotifyUserJoinedChatAsync(chatGuid, userId, user.Username, chat.Name);
            }
            
            logger.LogInformation("User {UserId} manually joined chat {ChatId}", userId, chatGuid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error joining chat {ChatId} for user {UserId}", chatGuid, userId);
            await Clients.Caller.SendAsync("Error", $"Failed to join chat: {ex.Message}");
        }
    }

    public async Task LeaveChat(string chatId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Invalid user token");
            return;
        }

        if (!Guid.TryParse(chatId, out var chatGuid))
        {
            await Clients.Caller.SendAsync("Error", "Invalid chat ID");
            return;
        }

        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ChatParticipants_{chatId}");
            
            await Clients.Caller.SendAsync("LeftChat", new
            {
                ChatId = chatGuid,
                LeftAt = DateTime.UtcNow
            });
            
            var user = await chatUsersRepository.GetByIdAsync(userId);
            var chat = await chatsRepository.GetByIdAsync(chatGuid);
            
            if (user != null && chat != null)
            {
                await notificationService.NotifyUserLeftChatAsync(chatGuid, userId, user.Username, chat.Name);
            }
            
            logger.LogInformation("User {UserId} manually left chat {ChatId}", userId, chatGuid);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error leaving chat {ChatId} for user {UserId}", chatGuid, userId);
            await Clients.Caller.SendAsync("Error", $"Failed to leave chat: {ex.Message}");
        }
    }

    public async Task SendMessage(string chatId, string message)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Invalid user token"); 
            return;
        }

        if (!Guid.TryParse(chatId, out var chatGuid))
        {
            await Clients.Caller.SendAsync("Error", "Invalid chat ID");
            return;
        }

        try
        {
            var command = new SendMessageCommand(chatGuid, userId, message);
            var result = await sender.SendAsync(command);

            if (result.IsSuccess)
            {
                await Clients.Caller.SendAsync("MessageSent", new
                {
                    MessageId = result.ObjectData,
                    ChatId = chatGuid,
                    SenderId = userId,
                    Content = message,
                    SentAt = DateTime.UtcNow
                });
                
                logger.LogInformation("User {UserId} sent message {MessageId} to chat {ChatId}", 
                    userId, result.ObjectData, chatGuid);
            }
            else
            {
                await Clients.Caller.SendAsync("Error", $"Failed to send message: {result.ObjectData?.ToString()}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending message to chat {ChatId} for user {UserId}", chatGuid, userId);
            await Clients.Caller.SendAsync("Error", $"Failed to send message: {ex.Message}");
        }
    }

    public async Task StartTyping(string chatId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return;

        await Clients.OthersInGroup($"Chat_{chatId}").SendAsync("UserTyping", new
        {
            ChatId = chatId,
            UserId = userId,
            IsTyping = true,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task StopTyping(string chatId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return;

        await Clients.OthersInGroup($"Chat_{chatId}").SendAsync("UserTyping", new
        {
            ChatId = chatId,
            UserId = userId,
            IsTyping = false,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task MarkMessageAsRead(string chatId, string messageId)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return;

        if (!Guid.TryParse(messageId, out var messageGuid)) return;

        await Clients.OthersInGroup($"Chat_{chatId}").SendAsync("MessageRead", new
        {
            ChatId = chatId,
            UserId = userId,
            MessageId = messageGuid,
            ReadAt = DateTime.UtcNow
        });
    }
} 