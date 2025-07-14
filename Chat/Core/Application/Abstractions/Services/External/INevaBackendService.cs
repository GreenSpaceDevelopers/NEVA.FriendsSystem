namespace Application.Abstractions.Services.External;

public interface INevaBackendService
{
    Task<bool> UpdatePlayerAsync(Guid playerId, string userName, CancellationToken cancellationToken = default);

    Task<bool> SendNotificationAsync(
        Guid templateId,
        Guid receiverId,
        Guid? senderId,
        bool forceRemove,
        List<string> receiverTemplateParams,
        List<string>? senderTemplateParams = null,
        CancellationToken cancellationToken = default);
} 