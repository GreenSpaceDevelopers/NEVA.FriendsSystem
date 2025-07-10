using Application.Dtos.Responses.BlackList;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class BlackList
{
    public static BlackListItemDto ToBlackListItem(this ChatUser chat)
    {
        return new BlackListItemDto(chat.AspNetUser.Id, chat.Username, chat.AspNetUser.Email, chat.Avatar?.Url);
    }

    public static async Task<BlackListItemDto> ToBlackListItemAsync(this ChatUser chat, IFilesSigningService filesSigningService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl = null;
        if (!string.IsNullOrEmpty(chat.Avatar?.Url))
        {
            avatarUrl = await filesSigningService.GetSignedUrlAsync(chat.Avatar.Url, cancellationToken);
        }

        return new BlackListItemDto(chat.AspNetUser.Id, chat.Username, chat.AspNetUser.Email, avatarUrl);
    }
}