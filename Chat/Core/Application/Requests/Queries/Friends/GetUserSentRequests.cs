using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Friends;

namespace Application.Requests.Queries.Friends;

public record GetUserSentRequests(Guid RequestedUserId, PageSettings PageSettings) : IRequest;

public record GetUserPendingRequests(Guid RequestedUserId, PageSettings PageSettings) : IRequest;

public class GetUserSentRequestsQueryHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<GetUserSentRequests>
{
    public async Task<IOperationResult> HandleAsync(GetUserSentRequests request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.RequestedUserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }
        
        var count = user.WaitingFriendRequests.Count;
        var sentRequests = user.WaitingFriendRequests
            .OrderBy(f => f.Username)
            .Skip(request.PageSettings.Skip)
            .Take(request.PageSettings.Take)
            .Select(f => new FriendRequestsDto(
                user.Id,
                f.Id
            ))
            .ToList();
        
        return ResultsHelper.Ok(new {sentRequests, count});
    }
}

public class GetUserPendingRequestsQueryHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<GetUserPendingRequests>
{
    public async Task<IOperationResult> HandleAsync(GetUserPendingRequests request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.RequestedUserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }
        
        var count = user.FriendRequests.Count;
        var sentRequests = user.FriendRequests
            .OrderBy(f => f.Username)
            .Skip(request.PageSettings.Skip)
            .Take(request.PageSettings.Take)
            .Select(f => new FriendRequestsDto(
                f.Id,
                user.Id
            ))
            .ToList();
        
        return ResultsHelper.Ok(new {sentRequests, count});
    }
}

