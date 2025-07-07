using System.Text;
using Application.Abstractions.Services.Notifications;
using GS.CommonLibrary.Protocol;
using Newtonsoft.Json;

namespace Infrastructure.Services.Communications;

internal class BackendNotificationService : CommonHttpClient, IBackendNotificationService
{
    private const string NotificationsControllerName = "Notifications";

    public BackendNotificationService()
    {
        ConString = Environment.GetEnvironmentVariable("Routes__Backend") ?? string.Empty;
    }

    protected override string ConString { get; set; }

    public async Task SendNotificationAsync(Guid templateId, Guid receiverId, Guid? senderId, bool forceRemove,
        List<string> receiverTemplateParams, List<string>? senderTemplateParams = null)
    {
        var model = new
        {
            TemplateId = templateId,
            ReceiverId = receiverId,
            SenderId = senderId,
            ForceRemove = forceRemove,
            RecieverTemplateParams = receiverTemplateParams,
            SenderTemplateParams = senderTemplateParams
        };

        var json = JsonConvert.SerializeObject(model);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        await PostAsync(NotificationsControllerName, "CreateNotificationByTemplate", data);
    }
} 