using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Friends;

namespace Application.Requests.Queries.Friends;

public record GetFriendsListQuery(Guid UserId, PageSettings PageSettings) : IRequest;

public class GetFriendsListQueryHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<GetFriendsListQuery>
{
    public async Task<IOperationResult> HandleAsync(GetFriendsListQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var friends = user.Friends
            .OrderBy(f => f.Username)
            .Skip((request.PageSettings.PageNumber - 1) * request.PageSettings.PageSize)
            .Take(request.PageSettings.PageSize)
            .Select(f => new FriendDto(
                f.Id,
                f.Username,
                f.Avatar?.Url,
                f.LastSeen
            ))
            .ToList();

        return ResultsHelper.Ok(friends);
    }
}