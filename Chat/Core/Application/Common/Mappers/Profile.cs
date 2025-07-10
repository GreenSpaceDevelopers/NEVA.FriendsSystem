using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Dtos.Responses.Profile;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class Profile
{
    public static ProfileDto ToProfileDto(this ChatUser user, bool canViewFullProfile, UserPrivacySettingsDto privacySettings, bool isBlockedByMe, bool hasBlockedMe, bool isFriendRequestSentByMe, bool isFriend, Guid? chatId, bool isChatDisabled, bool isChatMuted)
    {
        return new ProfileDto(
            user.Id,
            user.Username,
            canViewFullProfile ? user.AspNetUser.Email : null,
            canViewFullProfile ? user.Name : null,
            canViewFullProfile ? user.Surname : null,
            canViewFullProfile ? user.MiddleName : null,
            canViewFullProfile ? user.DateOfBirth?.ToUniversalTime() : null,
            canViewFullProfile ? user.Avatar?.Url : null,
            canViewFullProfile ? user.Cover?.Url : null,
            privacySettings,
            isBlockedByMe,
            hasBlockedMe,
            isFriendRequestSentByMe,
            isFriend,
            chatId,
            isChatDisabled,
            isChatMuted
        );
    }

    public static async Task<ProfileDto> ToProfileDtoAsync(this ChatUser user, bool canViewFullProfile, UserPrivacySettingsDto privacySettings, bool isBlockedByMe, bool hasBlockedMe, bool isFriendRequestSentByMe, bool isFriend, Guid? chatId, bool isChatDisabled, bool isChatMuted, IFilesSigningService filesSigningService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl = null;
        string? coverUrl = null;
        
        if (canViewFullProfile && !string.IsNullOrEmpty(user.Avatar?.Url))
        {
            avatarUrl = await filesSigningService.GetSignedUrlAsync(user.Avatar.Url, cancellationToken);
        }
        
        if (canViewFullProfile && !string.IsNullOrEmpty(user.Cover?.Url))
        {
            coverUrl = await filesSigningService.GetSignedUrlAsync(user.Cover.Url, cancellationToken);
        }

        return new ProfileDto(
            user.Id,
            user.Username,
            canViewFullProfile ? user.AspNetUser.Email : null,
            canViewFullProfile ? user.Name : null,
            canViewFullProfile ? user.Surname : null,
            canViewFullProfile ? user.MiddleName : null,
            canViewFullProfile ? user.DateOfBirth?.ToUniversalTime() : null,
            avatarUrl,
            coverUrl,
            privacySettings,
            isBlockedByMe,
            hasBlockedMe,
            isFriendRequestSentByMe,
            isFriend,
            chatId,
            isChatDisabled,
            isChatMuted
        );
    }

    public static OwnProfileDto ToOwnProfileDto(this ChatUser user)
    {
        return new OwnProfileDto(
            user.Id,
            user.Username,
            user.AspNetUser.Email,
            user.Name,
            user.Surname,
            user.MiddleName,
            user.DateOfBirth?.ToUniversalTime(),
            user.Avatar?.Url,
            user.Cover?.Url,
            new UserPrivacySettingsDto(
                user.PrivacySettings.Id,
                user.PrivacySettings.FriendsListVisibility,
                user.PrivacySettings.CommentsPermission,
                user.PrivacySettings.DirectMessagesPermission
            )
        );
    }

    public static async Task<OwnProfileDto> ToOwnProfileDtoAsync(this ChatUser user, IFilesSigningService filesSigningService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl = null;
        string? coverUrl = null;
        
        if (!string.IsNullOrEmpty(user.Avatar?.Url))
        {
            avatarUrl = await filesSigningService.GetSignedUrlAsync(user.Avatar.Url, cancellationToken);
        }
        
        if (!string.IsNullOrEmpty(user.Cover?.Url))
        {
            coverUrl = await filesSigningService.GetSignedUrlAsync(user.Cover.Url, cancellationToken);
        }

        return new OwnProfileDto(
            user.Id,
            user.Username,
            user.AspNetUser.Email,
            user.Name,
            user.Surname,
            user.MiddleName,
            user.DateOfBirth?.ToUniversalTime(),
            avatarUrl,
            coverUrl,
            new UserPrivacySettingsDto(
                user.PrivacySettings.Id,
                user.PrivacySettings.FriendsListVisibility,
                user.PrivacySettings.CommentsPermission,
                user.PrivacySettings.DirectMessagesPermission
            )
        );
    }
}