namespace Application.Abstractions.Services.Notifications;

public interface IBackendNotificationService
{
    Task SendNotificationAsync(
        Guid templateId,
        Guid receiverId,
        Guid? senderId,
        bool forceRemove,
        List<string> receiverTemplateParams,
        List<string>? senderTemplateParams = null);
} 