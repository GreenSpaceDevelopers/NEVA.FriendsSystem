using Application.Dtos.Responses.BlackList;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class BlackList
{
    public static BlackListItemDto ToBlackListItem(this ChatUser chat)
    {
        return new BlackListItemDto(chat.AspNetUser.Id, chat.Username, chat.AspNetUser.Email, chat.Avatar?.Url);
    }
}