using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.External;
using Application.Dtos.Responses.Profile;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class Profile
{
    public static ProfileDto ToProfileDto(this ChatUser user, bool canViewFullProfile, UserPrivacySettingsDto privacySettings, bool isBlockedByMe, bool hasBlockedMe, bool isFriendRequestSentByMe, bool isFriend, Guid? chatId, bool isChatDisabled, bool isChatMuted, IReadOnlyList<LinkedAccountDto> linkedAccounts)
    {
        return new ProfileDto(
            user.Id,
            user.Username,
            user.PersonalLink,
            canViewFullProfile ? user.AspNetUser.Email : null,
            canViewFullProfile ? user.Name : null,
            canViewFullProfile ? user.Surname : null,
            canViewFullProfile ? user.MiddleName : null,
            canViewFullProfile ? user.DateOfBirth?.ToUniversalTime() : null,
            canViewFullProfile ? user.Avatar?.Url : "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png",
            canViewFullProfile ? user.Cover?.Url : null,
            privacySettings,
            isBlockedByMe,
            hasBlockedMe,
            isFriendRequestSentByMe,
            isFriend,
            chatId,
            isChatDisabled,
            isChatMuted,
            linkedAccounts
        );
    }

    public static async Task<ProfileDto> ToProfileDtoAsync(this ChatUser user, bool canViewFullProfile, UserPrivacySettingsDto privacySettings, bool isBlockedByMe, bool hasBlockedMe, bool isFriendRequestSentByMe, bool isFriend, Guid? chatId, bool isChatDisabled, bool isChatMuted, IFilesSigningService filesSigningService, ILinkedAccountsService linkedAccountsService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl = null;
        string? coverUrl = null;
        
        if (canViewFullProfile && !string.IsNullOrEmpty(user.Avatar?.Url))
        {
            avatarUrl = await filesSigningService.GetSignedUrlAsync(user.Avatar.Url, cancellationToken);
        }
        else
        {
            avatarUrl = "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png";
        }
        
        if (canViewFullProfile && !string.IsNullOrEmpty(user.Cover?.Url))
        {
            coverUrl = await filesSigningService.GetSignedUrlAsync(user.Cover.Url, cancellationToken);
        }

        var linkedAccounts = canViewFullProfile 
            ? await linkedAccountsService.GetLinkedAccountsAsync(user.Id, cancellationToken)
            : Array.Empty<LinkedAccountDto>();

        return new ProfileDto(
            user.Id,
            user.Username,
            user.PersonalLink,
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
            isChatMuted,
            linkedAccounts
        );
    }

    public static OwnProfileDto ToOwnProfileDto(this ChatUser user, bool hasUnreadMessages, bool hasPendingFriendRequests, IReadOnlyList<LinkedAccountDto> linkedAccounts)
    {
        return new OwnProfileDto(
            user.Id,
            user.Username,
            user.PersonalLink,
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
            ),
            hasUnreadMessages,
            hasPendingFriendRequests,
            linkedAccounts
        );
    }

    public static async Task<OwnProfileDto> ToOwnProfileDtoAsync(this ChatUser user, bool hasUnreadMessages, bool hasPendingFriendRequests, IFilesSigningService filesSigningService, ILinkedAccountsService linkedAccountsService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl;
        string? coverUrl = null;
        
        if (!string.IsNullOrEmpty(user.Avatar?.Url))
        {
            avatarUrl = await filesSigningService.GetSignedUrlAsync(user.Avatar.Url, cancellationToken);
        }
        else
        {
            avatarUrl = "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png";
        }
        
        if (!string.IsNullOrEmpty(user.Cover?.Url))
        {
            coverUrl = await filesSigningService.GetSignedUrlAsync(user.Cover.Url, cancellationToken);
        }

        var linkedAccounts = await linkedAccountsService.GetLinkedAccountsAsync(user.Id, cancellationToken);

        return new OwnProfileDto(
            user.Id,
            user.Username,
            user.PersonalLink,
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
            ),
            hasUnreadMessages,
            hasPendingFriendRequests,
            linkedAccounts
        );
    }
}