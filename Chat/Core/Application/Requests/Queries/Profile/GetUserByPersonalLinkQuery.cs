using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.External;
using Application.Common.Mappers;
using Application.Dtos.Responses.Profile;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Users;

namespace Application.Requests.Queries.Profile;

public record GetUserByPersonalLinkQuery(string PersonalLink, Guid CurrentUserId) : IRequest;

public class GetUserByPersonalLinkQueryHandler(IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService, ILinkedAccountsService linkedAccountsService) : IRequestHandler<GetUserByPersonalLinkQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserByPersonalLinkQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByPersonalLinkWithFriendsAsync(request.PersonalLink, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound($"User with personal link '{request.PersonalLink}' not found");
        }

        var isFriend = user.Friends.Any(f => f.Id == request.CurrentUserId);
        var isFriendRequestSentByMe = user.FriendRequests.Any(fr => fr.Id == request.CurrentUserId);

        var canViewFullProfile = user.Id == request.CurrentUserId ||
                                user.PrivacySettings.FriendsListVisibility == PrivacyLevel.Public;

        var isBlockedByMe = await chatUsersRepository.IsUserBlockedByAsync(user.Id, request.CurrentUserId, cancellationToken);
        var hasBlockedMe = await chatUsersRepository.IsUserBlockedByAsync(request.CurrentUserId, user.Id, cancellationToken);

        if (hasBlockedMe)
        {
            canViewFullProfile = false;
        }

        UserChatInfo? chatInfo = null;
        if (user.Id != request.CurrentUserId)
        {
            chatInfo = await chatUsersRepository.GetChatInfoBetweenUsersAsync(request.CurrentUserId, user.Id, cancellationToken);
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
            linkedAccountsService,
            cancellationToken);

        return ResultsHelper.Ok(profileDto);
    }
} 