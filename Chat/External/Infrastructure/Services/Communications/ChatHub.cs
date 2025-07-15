using Microsoft.AspNetCore.SignalR;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.Communications;
using Microsoft.Extensions.Logging;
using GS.IdentityServerApi;
using GS.IdentityServerApi.Protocol.Models;

namespace Infrastructure.Services.Communications;

public class ChatHub(IChatsRepository chatsRepository, IChatUsersRepository chatUsersRepository, IChatNotificationService notificationService, ILogger<ChatHub> logger, IdentityClient identityClient) : Hub
{
    private async Task<Guid> GetCurrentUserIdAsync()
    {
        if (Context.Items.TryGetValue("CachedUserId", out var cachedUserId) && cachedUserId is Guid userId)
        {
            return userId;
        }

        var http = Context.GetHttpContext();
        if (http is null) 
        {
            logger.LogWarning("GetCurrentUserId: HttpContext is null");
            return Guid.Empty;
        }

        if (http.Items["SessionContext"] is IdentitySession identitySession)
        {
            var userIdFromSession = identitySession.User.UserId;
            logger.LogInformation("Got real userId from SessionContext: {UserId}", userIdFromSession);
            
            Context.Items["CachedUserId"] = userIdFromSession;
            return userIdFromSession;
        }

        logger.LogInformation("SessionContext not found, trying to get token manually...");

        string? sessionToken = null;

        if (http.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var authVal = authHeader.ToString();
            if (!string.IsNullOrWhiteSpace(authVal) && authVal.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                sessionToken = authVal[7..].Trim();
                logger.LogInformation("Extracted Bearer token: {Token}", sessionToken);
            }
        }

        if (string.IsNullOrEmpty(sessionToken) && http.Request.Query.TryGetValue("access_token", out var tokenVals))
        {
            sessionToken = tokenVals.FirstOrDefault();
            logger.LogInformation("Found access_token query parameter: {Token}", sessionToken);
        }

        if (string.IsNullOrEmpty(sessionToken))
        {
            logger.LogWarning("No token found in request");
            return Guid.Empty;
        }

        if (!Guid.TryParse(sessionToken, out var sessionGuid))
        {
            logger.LogWarning("Invalid session token format: {Token}", sessionToken);
            return Guid.Empty;
        }

        try
        {
            logger.LogInformation("Calling IdentityClient to get real userId for session: {SessionId}", sessionGuid);
            var responseModel = await identityClient.GetUserSessionAsync(sessionGuid);
            
            if (responseModel is { Success: true })
            {
                var realUserId = responseModel.Data?.User.UserId ?? Guid.Empty;
                logger.LogInformation("Successfully got real userId from IdentityClient: {UserId} for session {SessionId}", realUserId, sessionGuid);
                
                Context.Items["CachedUserId"] = realUserId;
                return realUserId;
            }
            else
            {
                logger.LogWarning("IdentityClient returned failure for session {SessionId}", sessionGuid);
                return Guid.Empty;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling IdentityClient for session {SessionId}", sessionGuid);
            return Guid.Empty;
        }
    }

    public override async Task OnConnectedAsync()
    {
        logger.LogInformation("New SignalR connection attempt: {ConnectionId}", Context.ConnectionId);
        
        var userId = await GetCurrentUserIdAsync();
        logger.LogInformation("Extracted userId: {UserId} from connection {ConnectionId}", userId, Context.ConnectionId);
        
        if (userId == Guid.Empty) 
        { 
            logger.LogWarning("Invalid user token for connection {ConnectionId}", Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", "Invalid user token"); 
            return; 
        }

        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            logger.LogInformation("Added user {UserId} to User_{UserId} group", userId, userId);
            
            logger.LogInformation("Getting chats for user {UserId}...", userId);
            
            List<Guid> userChatIds;
            try
            {
                userChatIds = await chatsRepository.GetUserChatIdsAsync(userId);
                logger.LogInformation("Found {ChatCount} chats for user {UserId}: [{ChatIds}]", 
                    userChatIds.Count, userId, string.Join(", ", userChatIds));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting chats for user {UserId}", userId);
                userChatIds = new List<Guid>(); // Fallback to empty list
            }
            
            var joinTasks = new List<Task>();
            foreach (var chatId in userChatIds)
            {
                logger.LogInformation("Adding user {UserId} to Chat_{ChatId} and ChatParticipants groups", 
                    userId, chatId);
                joinTasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}"));
                joinTasks.Add(Groups.AddToGroupAsync(Context.ConnectionId, $"ChatParticipants_{chatId}"));
            }
            
            logger.LogInformation("Executing {TaskCount} group join tasks...", joinTasks.Count);
            await Task.WhenAll(joinTasks);
            logger.LogInformation("All group join tasks completed");
            
            logger.LogInformation("User {UserId} connected with {ConnectionId} and joined {ChatCount} chats", 
                userId, Context.ConnectionId, userChatIds.Count);

            logger.LogInformation("Sending Connected event to client...");
            await Clients.Caller.SendAsync("Connected", new
            {
                UserId = userId,
                ConnectionId = Context.ConnectionId,
                ConnectedAt = DateTime.UtcNow,
                JoinedChatsCount = userChatIds.Count,
                JoinedChatIds = userChatIds
            });
            logger.LogInformation("Connected event sent successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error connecting user {UserId} with connection {ConnectionId}", userId, Context.ConnectionId);
            await Clients.Caller.SendAsync("Error", $"Connection failed: {ex.Message}");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = await GetCurrentUserIdAsync();
        
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
        var userId = await GetCurrentUserIdAsync();
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
        var userId = await GetCurrentUserIdAsync();
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





    public async Task MarkMessageAsRead(string chatId, string messageId)
    {
        var userId = await GetCurrentUserIdAsync();
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