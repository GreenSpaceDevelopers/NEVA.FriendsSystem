using Application.Dtos.Responses.Profile;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class Profile
{
    public static ProfileDto ToProfileDto(this ChatUser user, bool canViewFullProfile)
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
            new UserPrivacySettingsDto(
                user.PrivacySettings.Id,
                user.PrivacySettings.FriendsListVisibility,
                user.PrivacySettings.CommentsPermission,
                user.PrivacySettings.DirectMessagesPermission
            )
        );
    }
}