using Application.Dtos.Responses.BlackList;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class BlackList
{
    public static BlackListItemDto ToBlackListItem(this ChatUser chat)
    {
        var userId = chat.AspNetUser?.Id ?? chat.Id;
        var email = chat.AspNetUser?.Email ?? string.Empty;
        return new BlackListItemDto(userId, chat.Username, chat.PersonalLink, email, chat.Avatar?.Url);
    }

    public static async Task<BlackListItemDto> ToBlackListItemAsync(this ChatUser chat, IFilesSigningService filesSigningService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl = null;
        if (!string.IsNullOrEmpty(chat.Avatar?.Url))
        {
            avatarUrl = await filesSigningService.GetSignedUrlForObjectAsync(chat.Avatar.Url, chat.Avatar.BucketName ?? "neva-avatars", cancellationToken);
        }
        else
        {
            avatarUrl = "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png";
        }

        var userId2 = chat.AspNetUser?.Id ?? chat.Id;
        var email2 = chat.AspNetUser?.Email ?? string.Empty;
        return new BlackListItemDto(userId2, chat.Username, chat.PersonalLink, email2, avatarUrl);
    }
}