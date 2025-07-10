namespace Application.Abstractions.Services.Communications;

public interface IChatNotificationService
{
    Task NotifyNewMessageAsync(Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt);
    Task NotifyNewMessageWithAttachmentAsync(Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt);
    
    Task NotifyUsersAboutNewMessageAsync(IEnumerable<Guid> userIds, Guid chatId, Guid senderId, string senderName, string message, DateTime sentAt, string? chatName = null);
    Task NotifyUsersAboutNewMessageWithAttachmentAsync(IEnumerable<Guid> userIds, Guid chatId, Guid senderId, string senderName, string message, string? attachmentUrl, DateTime sentAt, string? chatName = null);
} 