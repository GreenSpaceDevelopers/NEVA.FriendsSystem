using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Friends;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Friends;

public record GetUserSentRequests(Guid RequestedUserId, PageSettings PageSettings) : IRequest;

public record GetUserPendingRequests(Guid RequestedUserId, PageSettings PageSettings) : IRequest;

public class GetUserSentRequestsQueryHandler(IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetUserSentRequests>
{
    public async Task<IOperationResult> HandleAsync(GetUserSentRequests request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsNoTrackingAsync(request.RequestedUserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }
        
        var waitingFriends = user.WaitingFriendRequests
            .OrderBy(f => f.Username)
            .Skip(request.PageSettings.Skip)
            .Take(request.PageSettings.Take)
            .ToList();

        var friendRequestDtos = new List<FriendRequestsDto>();
        foreach (var friend in waitingFriends)
        {
            string? avatarUrl = null;
            if (!string.IsNullOrEmpty(friend.Avatar?.Url))
            {
                avatarUrl = await filesSigningService.GetSignedUrlAsync(friend.Avatar.Url, cancellationToken);
            }

            friendRequestDtos.Add(new FriendRequestsDto(
                user.Id,
                friend.Id,
                friend.Username,
                avatarUrl
            ));
        }
        
        var pagedResult = new PagedList<FriendRequestsDto>
        {
            TotalCount = user.WaitingFriendRequests.Count,
            Data = friendRequestDtos
        };
        return ResultsHelper.Ok(pagedResult);
    }
}

public class GetUserPendingRequestsQueryHandler(IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetUserPendingRequests>
{
    public async Task<IOperationResult> HandleAsync(GetUserPendingRequests request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsNoTrackingAsync(request.RequestedUserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }
        
        var pendingFriends = user.FriendRequests
            .OrderBy(f => f.Username)
            .Skip(request.PageSettings.Skip)
            .Take(request.PageSettings.Take)
            .ToList();

        var friendRequestDtos = new List<FriendRequestsDto>();
        foreach (var friend in pendingFriends)
        {
            string? avatarUrl = null;
            if (!string.IsNullOrEmpty(friend.Avatar?.Url))
            {
                avatarUrl = await filesSigningService.GetSignedUrlAsync(friend.Avatar.Url, cancellationToken);
            }

            friendRequestDtos.Add(new FriendRequestsDto(
                friend.Id,
                user.Id,
                friend.Username,
                avatarUrl
            ));
        }
        
        var pagedResult = new PagedList<FriendRequestsDto>
        {
            TotalCount = user.FriendRequests.Count,
            Data = friendRequestDtos
        };
        return ResultsHelper.Ok(pagedResult);
    }
}

