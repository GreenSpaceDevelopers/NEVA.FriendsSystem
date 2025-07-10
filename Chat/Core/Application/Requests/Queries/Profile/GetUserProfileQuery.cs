using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Common.Mappers;
using Application.Dtos.Responses.Profile;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Users;

namespace Application.Requests.Queries.Profile;

public record GetUserProfileQuery(Guid RequestedUserId, Guid CurrentUserId) : IRequest;

public class GetUserProfileQueryHandler(IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetUserProfileQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserProfileQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdWithFriendsAsNoTrackingAsync(request.RequestedUserId, cancellationToken);
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

        var privacySettings = hasBlockedMe 
            ? new UserPrivacySettingsDto(Guid.Empty, PrivacyLevel.Private, PrivacyLevel.Private, PrivacyLevel.Private)
            : new UserPrivacySettingsDto(
                user.PrivacySettings.Id,
                user.PrivacySettings.FriendsListVisibility,
                user.PrivacySettings.CommentsPermission,
                user.PrivacySettings.DirectMessagesPermission
            );

        var profileDto = await user.ToProfileDtoAsync(
            canViewFullProfile,
            privacySettings,
            isBlockedByMe,
            hasBlockedMe,
            isFriendRequestSentByMe,
            isFriend,
            chatInfo?.ChatId,
            chatInfo?.IsChatDisabled ?? false,
            chatInfo?.IsChatMuted ?? false,
            filesSigningService,
            cancellationToken);

        return ResultsHelper.Ok(profileDto);
    }
}