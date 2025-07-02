using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Friends;
using Application.Services.ApplicationInfrastructure.Results;

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
        
        var pagedResult = new Common.Models.PagedList<FriendRequestsDto>
        {
            TotalCount = user.WaitingFriendRequests.Count,
            Data = user.WaitingFriendRequests
                .OrderBy(f => f.Username)
                .Skip(request.PageSettings.Skip)
                .Take(request.PageSettings.Take)
                .Select(f => new FriendRequestsDto(
                    user.Id,
                    f.Id
                ))
                .ToList()
        };
        return ResultsHelper.Ok(pagedResult);
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
        
        var pagedResult = new Common.Models.PagedList<FriendRequestsDto>
        {
            TotalCount = user.FriendRequests.Count,
            Data = user.FriendRequests
                .OrderBy(f => f.Username)
                .Skip(request.PageSettings.Skip)
                .Take(request.PageSettings.Take)
                .Select(f => new FriendRequestsDto(
                    f.Id,
                    user.Id
                ))
                .ToList()
        };
        return ResultsHelper.Ok(pagedResult);
    }
}

