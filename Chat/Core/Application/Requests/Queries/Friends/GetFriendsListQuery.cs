using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Friends;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Application.Dtos.Responses.Match;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Friends;

public record GetFriendsListQuery(Guid UserId, PageSettings PageSettings, Guid? DisciplineId) : IRequest;

public class GetFriendsListQueryHandler(IChatUsersRepository chatUsersRepository, IConfiguration configuration, HttpClient httpClient) : IRequestHandler<GetFriendsListQuery>
{
    public async Task<IOperationResult> HandleAsync(GetFriendsListQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var friends = user.Friends.OrderBy(f => f.Username).ToList();

        if (request.DisciplineId.HasValue)
        {
            var friendsInDiscipline = new List<Domain.Models.Users.ChatUser>();
            
            foreach (var friend in friends)
            {
                var friendDisciplines = await GetUserDisciplinesFromMatchHistory(friend.Id, cancellationToken);
                if (friendDisciplines.Contains(request.DisciplineId.Value))
                {
                    friendsInDiscipline.Add(friend);
                }
            }
            
            friends = friendsInDiscipline;
        }

        var totalCount = friends.Count;
        var pagedFriends = friends
            .Skip(request.PageSettings.Skip)
            .Take(request.PageSettings.Take)
            .ToList();
        var pagedResult = new Common.Models.PagedList<FriendDto>
        {
            TotalCount = totalCount,
            Data = pagedFriends.Select(f => new FriendDto(
                f.Id,
                f.Username,
                f.Avatar?.Url,
                f.LastSeen
            )).ToList()
        };
        return ResultsHelper.Ok(pagedResult);
    }

    private async Task<List<Guid>> GetUserDisciplinesFromMatchHistory(Guid playerId, CancellationToken cancellationToken)
    {
        try
        {
            var externalApiBaseUrl = configuration["ExternalApi:BaseUrl"] ?? "http://localhost:7198";
            var externalApiUrl = $"{externalApiBaseUrl}/api/Players/GetMatchHistory/{playerId}";
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