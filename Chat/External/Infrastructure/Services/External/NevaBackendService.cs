using System.Text;
using Application.Abstractions.Services.External;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Infrastructure.Services.External;

public class NevaBackendService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<NevaBackendService> logger)
    : INevaBackendService
{
    private readonly string _baseUrl = configuration["ExternalApi:BaseUrl"] ?? throw new InvalidOperationException("ExternalApi:BaseUrl not configured");

    public async Task<bool> UpdatePlayerAsync(Guid playerId, string userName, CancellationToken cancellationToken = default)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(playerId.ToString()), "UserId");
            content.Add(new StringContent(userName), "Name");

            var response = await httpClient.PutAsync($"{_baseUrl}/Players/UpdatePlayer/{playerId}", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Successfully updated player {PlayerId} with username {UserName} in NEVA Core", playerId, userName);
                return true;
            }
            
            logger.LogWarning("Failed to update player {PlayerId} in NEVA Core. Status: {StatusCode}", playerId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating player {PlayerId} with username {UserName} in NEVA Core", playerId, userName);
            return false;
        }
    }

    public async Task<bool> SendNotificationAsync(
        Guid templateId, 
        Guid receiverId, 
        Guid? senderId, 
        bool forceRemove, 
        List<string> receiverTemplateParams, 
        List<string>? senderTemplateParams = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/Notifications/CreateNotificationByTemplate";

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

            var response = await httpClient.PostAsync(url, data, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                logger.LogInformation("Successfully sent notification with template {TemplateId} to receiver {ReceiverId}", templateId, receiverId);
                return true;
            }
            
            logger.LogWarning("Failed to send notification with template {TemplateId} to receiver {ReceiverId}. Status: {StatusCode}", templateId, receiverId, response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending notification with template {TemplateId} to receiver {ReceiverId}", templateId, receiverId);
            return false;
        }
    }
} 