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
            canViewFullProfile ? user.Avatar?.Url : null,
            canViewFullProfile ? user.Cover?.Url : null,
            (PrivacySettingsEnums)user.PrivacySetting.Id
        );
    }
} 