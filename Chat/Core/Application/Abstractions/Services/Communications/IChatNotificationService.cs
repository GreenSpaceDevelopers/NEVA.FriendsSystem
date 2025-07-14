namespace Application.Abstractions.Services.Communications;

public interface IChatNotificationService
{
    Task NotifyNewMessageAsync(Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt);
    Task NotifyNewMessageWithAttachmentAsync(Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt);
    
    Task NotifyUsersAboutNewMessageAsync(IEnumerable<Guid> userIds, Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt, string? chatName = null);
    Task NotifyUsersAboutNewMessageWithAttachmentAsync(IEnumerable<Guid> userIds, Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt, string? chatName = null);
    
    Task NotifyNewMessageToAllChatParticipantsAsync(Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt, Guid messageId, string? chatName = null);
    Task NotifyNewMessageWithAttachmentToAllChatParticipantsAsync(Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt, Guid messageId, string? chatName = null);
    
    Task NotifyUserJoinedChatAsync(Guid chatId, Guid userId, string username, string? chatName = null);
    Task NotifyUserLeftChatAsync(Guid chatId, Guid userId, string username, string? chatName = null);
    Task NotifyUserTypingAsync(Guid chatId, Guid userId, string username, bool isTyping);
    
    Task NotifyMessageReadAsync(Guid chatId, Guid userId, Guid messageId);
} 