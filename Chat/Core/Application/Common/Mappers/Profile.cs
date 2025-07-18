using Application.Dtos.Responses.Profile;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class Profile
{
    public static ProfileDto ToProfileDto(this ChatUser user, bool canViewFullProfile, bool isBlockedByMe = false, bool hasBlockedMe = false)
    {
        return user.ToProfileDto(
            canViewFullProfile,
            includePrivacySettings: !hasBlockedMe,
            isBlockedByMe: isBlockedByMe,
            hasBlockedMe: hasBlockedMe);
    }

    public static ProfileDto ToProfileDto(this ChatUser user, bool canViewFullProfile, bool includePrivacySettings, bool isBlockedByMe = false, bool hasBlockedMe = false, bool isFriend = false, bool isFriendRequestSentByMe = false)
    {
        return new ProfileDto(
            user.Id,
            user.Username,
            canViewFullProfile ? user.Name : null,
            canViewFullProfile ? user.Surname : null,
            canViewFullProfile ? user.MiddleName : null,
            canViewFullProfile ? user.DateOfBirth : null,
            canViewFullProfile ? user.Avatar?.Url : null,
            canViewFullProfile ? user.Cover?.Url : null,
            includePrivacySettings ? new UserPrivacySettingsDto(
                user.PrivacySettings.Id,
                user.PrivacySettings.FriendsListVisibility,
                user.PrivacySettings.CommentsPermission,
                user.PrivacySettings.DirectMessagesPermission
            ) : new UserPrivacySettingsDto(Guid.Empty, PrivacyLevel.Private, PrivacyLevel.Private, PrivacyLevel.Private),
            isBlockedByMe,
            hasBlockedMe,
            isFriendRequestSentByMe,
            isFriend
        );
    }
}