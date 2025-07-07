namespace Application.Abstractions.Services.Communications;

public interface IChatNotificationService
{
    Task NotifyNewMessageAsync(Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt);
    Task NotifyUserJoinedChatAsync(Guid chatId, Guid userId, string userName);
    Task NotifyUserLeftChatAsync(Guid chatId, Guid userId, string userName);
} 