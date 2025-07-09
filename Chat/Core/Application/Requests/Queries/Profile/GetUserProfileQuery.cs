using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Users;

namespace Application.Requests.Queries.Profile;

public record GetUserProfileQuery(Guid RequestedUserId, Guid CurrentUserId) : IRequest;

public class GetUserProfileQueryHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<GetUserProfileQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserProfileQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsync(request.RequestedUserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound($"User not found. RequestedUserId: {request.RequestedUserId}");
        }

        var isFriend = user.Friends.Any(f => f.Id == request.CurrentUserId);
        var isFriendRequestSentByMe = user.FriendRequests.Any(fr => fr.Id == request.CurrentUserId);

        var canViewFullProfile = request.RequestedUserId == request.CurrentUserId ||
                                user.PrivacySettings.FriendsListVisibility == PrivacyLevel.Public;

        var isBlockedByMe = await chatUsersRepository.IsUserBlockedByAsync(request.RequestedUserId, request.CurrentUserId, cancellationToken);
        var hasBlockedMe = await chatUsersRepository.IsUserBlockedByAsync(request.CurrentUserId, request.RequestedUserId, cancellationToken);

        if (hasBlockedMe)
        {
            canViewFullProfile = false;
        }

        UserChatInfo? chatInfo = null;
        if (request.RequestedUserId != request.CurrentUserId)
        {
            chatInfo = await chatUsersRepository.GetChatInfoBetweenUsersAsync(request.CurrentUserId, request.RequestedUserId, cancellationToken);
        }

        var profileDto = user.ToProfileDto(
            canViewFullProfile,
            includePrivacySettings: !hasBlockedMe,
            isBlockedByMe: isBlockedByMe,
            hasBlockedMe: hasBlockedMe,
            isFriend: isFriend,
            isFriendRequestSentByMe: isFriendRequestSentByMe,
            chatInfo: chatInfo);

        return ResultsHelper.Ok(profileDto);
    }
}