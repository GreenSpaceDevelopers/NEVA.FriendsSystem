using System.Text;
using Application.Abstractions.Services.Notifications;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Infrastructure.Services.Communications;

internal class BackendNotificationService(IConfiguration configuration, HttpClient httpClient) : IBackendNotificationService
{
    private readonly string _backendBaseUrl = configuration["ExternalApi:BaseUrl"] ?? "http://localhost:7198";

    public async Task SendNotificationAsync(Guid templateId, Guid receiverId, Guid? senderId, bool forceRemove,
        List<string> receiverTemplateParams, List<string>? senderTemplateParams = null)
    {
        var url = $"{_backendBaseUrl}/Notifications/CreateNotificationByTemplate";

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

        try
        {
            await httpClient.PostAsync(url, data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BackendNotificationService] Error sending notification: {ex.Message}");
        }
    }
} 