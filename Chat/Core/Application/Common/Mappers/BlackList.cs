using Application.Dtos.Responses.BlackList;
using Domain.Models.Users;

namespace Application.Common.Mappers;

public static class BlackList
{
    public static BlackListItem ToBlackListItem(this ChatUser chat)
    {
        return new BlackListItem(chat.AspNetUser.Id, chat.Username, chat.AspNetUser.Email);
    }
}