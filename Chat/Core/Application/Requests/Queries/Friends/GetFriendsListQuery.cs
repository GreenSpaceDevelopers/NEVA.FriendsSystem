using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Friends;
using System.Text.Json;
using Application.Common.Models;
using Microsoft.Extensions.Configuration;
using Application.Dtos.Responses.Match;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Friends;

public record GetFriendsListQuery(Guid UserId, PageSettings PageSettings, Guid? DisciplineId, string? SearchQuery = null) : IRequest;

public class GetFriendsListQueryHandler(IChatUsersRepository chatUsersRepository, IConfiguration configuration, HttpClient httpClient, IFilesSigningService filesSigningService) : IRequestHandler<GetFriendsListQuery>
{
    public async Task<IOperationResult> HandleAsync(GetFriendsListQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var friendsWithBlockingInfo = await chatUsersRepository.GetFriendsWithBlockingInfoAsync(request.UserId, request.SearchQuery, cancellationToken);

        if (request.DisciplineId.HasValue)
        {
            var friendsInDiscipline = new List<UserWithBlockingInfo>();
            
            foreach (var friendInfo in friendsWithBlockingInfo)
            {
                var friendDisciplines = await GetUserDisciplinesFromMatchHistory(friendInfo.User.Id, cancellationToken);
                if (friendDisciplines.Contains(request.DisciplineId.Value))
                {
                    friendsInDiscipline.Add(friendInfo);
                }
            }
            
            friendsWithBlockingInfo = friendsInDiscipline;
        }

        var totalCount = friendsWithBlockingInfo.Count;
        var pagedFriends = friendsWithBlockingInfo
            .Skip(request.PageSettings.Skip)
            .Take(request.PageSettings.Take)
            .ToList();

        var friendDtos = new List<FriendDto>();
        
        foreach (var friendInfo in pagedFriends)
        {
            var chatInfo = await chatUsersRepository.GetChatInfoBetweenUsersAsync(request.UserId, friendInfo.User.Id, cancellationToken);
            
            string? avatarUrl = null;
            if (!string.IsNullOrEmpty(friendInfo.User.Avatar?.Url))
            {
                avatarUrl = await filesSigningService.GetSignedUrlAsync(friendInfo.User.Avatar.Url, cancellationToken);
            }
            
            var friendDto = new FriendDto(
                friendInfo.User.Id,
                friendInfo.User.Username,
                friendInfo.User.PersonalLink,
                avatarUrl,
                friendInfo.User.LastSeen,
                friendInfo.IsBlockedByMe,
                friendInfo.HasBlockedMe,
                chatInfo.ChatId,
                chatInfo.IsChatDisabled,
                chatInfo.IsChatMuted
            );
            
            friendDtos.Add(friendDto);
        }

        var pagedResult = new PagedList<FriendDto>
        {
            TotalCount = totalCount,
            Data = friendDtos
        };
        
        return ResultsHelper.Ok(pagedResult);
    }

    private async Task<List<Guid>> GetUserDisciplinesFromMatchHistory(Guid playerId, CancellationToken cancellationToken)
    {
        try
        {
            var externalApiBaseUrl = configuration["ExternalApi:BaseUrl"] ?? "http://localhost:7198";
            var externalApiUrl = $"{externalApiBaseUrl}/Players/GetMatchHistory/{playerId}";
            var response = await httpClient.GetAsync(externalApiUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return [];
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var matchHistoryResponse = JsonSerializer.Deserialize<MatchHistoryResponse>(jsonContent, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

            return matchHistoryResponse?.Data?.Data?.Select(m => m.GameTypeId).Distinct().ToList() ?? [];
        }
        catch
        {
            return [];
        }
    }
}