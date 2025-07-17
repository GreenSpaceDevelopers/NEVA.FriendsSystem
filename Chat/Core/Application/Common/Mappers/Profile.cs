using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.External;
using Application.Dtos.Responses.Profile;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class Profile
{
    public static async Task<ProfileDto> ToProfileDtoAsync(this ChatUser user, bool canViewFullProfile, UserPrivacySettingsDto privacySettings, bool isBlockedByMe, bool hasBlockedMe, bool isFriendRequestSentByMe, bool isFriend, Guid? chatId, bool isChatDisabled, bool isChatMuted, IFilesSigningService filesSigningService, ILinkedAccountsService linkedAccountsService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl = null;
        string? coverUrl = null;
        
        if (canViewFullProfile && user.Avatar != null)
        {
            avatarUrl = await GetSignedUrlAsync(user.Avatar, filesSigningService, cancellationToken);
        }
        else
        {
            avatarUrl = "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png";
        }
        
        if (canViewFullProfile && user.Cover != null)
        {
            coverUrl = await GetSignedUrlAsync(user.Cover, filesSigningService, cancellationToken);
        }

        var linkedAccounts = canViewFullProfile 
            ? await linkedAccountsService.GetLinkedAccountsAsync(user.Id, cancellationToken)
            : [];

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

    public static async Task<OwnProfileDto> ToOwnProfileDtoAsync(this ChatUser user, bool hasUnreadMessages, bool hasPendingFriendRequests, IFilesSigningService filesSigningService, ILinkedAccountsService linkedAccountsService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl;
        string? coverUrl = null;
        
        if (user.Avatar != null)
        {
            avatarUrl = await GetSignedUrlAsync(user.Avatar, filesSigningService, cancellationToken);
        }
        else
        {
            avatarUrl = "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png";
        }
        
        if (user.Cover != null)
        {
            coverUrl = await GetSignedUrlAsync(user.Cover, filesSigningService, cancellationToken);
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

    private static async Task<string?> GetSignedUrlAsync(Domain.Models.Messaging.Attachment attachment, IFilesSigningService filesSigningService, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(attachment.BucketName))
        {
            return attachment.Url;
        }
        
        return await filesSigningService.GetSignedUrlForObjectAsync(attachment.Url, attachment.BucketName, cancellationToken);
    }
}