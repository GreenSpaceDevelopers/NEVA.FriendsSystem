using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Profile;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Users;

namespace Application.Requests.Queries.Profile;

public record GetUserInteractionPermissionsQuery(Guid TargetUserId, Guid CurrentUserId) : IRequest;

public class GetUserInteractionPermissionsQueryHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<GetUserInteractionPermissionsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserInteractionPermissionsQuery request, CancellationToken cancellationToken = default)
    {
        var targetUser = await chatUsersRepository.GetByIdWithFriendsAsNoTrackingAsync(request.TargetUserId, cancellationToken);
        if (targetUser is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var hasBlockedMe = await chatUsersRepository.IsUserBlockedByAsync(request.CurrentUserId, request.TargetUserId, cancellationToken);
        if (hasBlockedMe)
        {
            var denied = new UserInteractionPermissionsDto(false, false, false);
            return ResultsHelper.Ok(denied);
        }

        var isOwner = request.CurrentUserId == request.TargetUserId;
        var isFriend = targetUser.Friends.Any(f => f.Id == request.CurrentUserId);

        var privacy = targetUser.PrivacySettings;
        var canWriteMessages = HasPermission(privacy?.DirectMessagesPermission ?? PrivacyLevel.Public, isFriend, isOwner);
        var canLeaveComments = HasPermission(privacy?.CommentsPermission ?? PrivacyLevel.Public, isFriend, isOwner);
        var canViewFriendLists = HasPermission(privacy?.FriendsListVisibility ?? PrivacyLevel.Public, isFriend, isOwner);

        var dto = new UserInteractionPermissionsDto(canWriteMessages, canLeaveComments, canViewFriendLists);
        return ResultsHelper.Ok(dto);
    }

    private static bool HasPermission(PrivacyLevel level, bool isFriend, bool isOwner)
    {
        return level switch
        {
            PrivacyLevel.Public => true,
            PrivacyLevel.Friends => isFriend || isOwner,
            PrivacyLevel.Private => isOwner,
            _ => false
        };
    }
} 