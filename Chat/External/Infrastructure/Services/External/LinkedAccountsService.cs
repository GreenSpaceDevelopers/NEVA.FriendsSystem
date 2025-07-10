using System.Text.Json;
using Application.Abstractions.Services.External;
using Application.Dtos.Responses.Profile;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.External;

public class LinkedAccountsService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<LinkedAccountsService> logger)
    : ILinkedAccountsService
{
    private readonly string _baseUrl = configuration["ExternalApi:BaseUrl"] ?? throw new InvalidOperationException("ExternalApi:BaseUrl not configured");

    public async Task<IReadOnlyList<LinkedAccountDto>> GetLinkedAccountsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync($"{_baseUrl}/Players/GetPlayerInfo?playerId={userId}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get player info for user {UserId}. Status: {StatusCode}", userId, response.StatusCode);
                return Array.Empty<LinkedAccountDto>();
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var playerInfoResponse = JsonSerializer.Deserialize<PlayerInfoApiResponse>(content, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });

            if (playerInfoResponse?.Data?.LinkedAccounts == null)
            {
                return Array.Empty<LinkedAccountDto>();
            }

            return playerInfoResponse.Data.LinkedAccounts
                .Select(MapToLinkedAccountDto)
                .ToList()
                .AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting linked accounts for user {UserId}", userId);
            return Array.Empty<LinkedAccountDto>();
        }
    }

    private static LinkedAccountDto MapToLinkedAccountDto(LinkedAccountApiDto apiDto)
    {
        var displayName = apiDto.TypeId switch
        {
            "c0214c74-8980-4cd2-8e04-1b46a48fb74c" => LinkedAccountTypes.Steam,
            "a5b5ed12-db5a-4ada-86b8-53a6a9e6c760" => LinkedAccountTypes.Discord,
            "f79aebb4-b0e3-44b1-85b6-9ec14b39a708" => LinkedAccountTypes.Telegram,
            _ => "Unknown"
        };

        var type = displayName.ToLowerInvariant();

        return new LinkedAccountDto(type, apiDto.LinkedData, displayName);
    }
}

public class PlayerInfoApiResponse
{
    public bool Success { get; set; }
    public PlayerPageModelDto? Data { get; set; }
}

public class PlayerPageModelDto
{
    public List<LinkedAccountApiDto> LinkedAccounts { get; set; } = new();
}

public class LinkedAccountApiDto
{
    public string UserId { get; set; } = string.Empty;
    public string TypeId { get; set; } = string.Empty;
    public string LinkedData { get; set; } = string.Empty;
} 