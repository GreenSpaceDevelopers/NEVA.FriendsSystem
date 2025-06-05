using Application.Dtos.Responses.Chats;
using Domain.Models.Messaging;

namespace Application.Common.Mappers;

public static class Chats
{
    public static UserChatListItem ToUserChatListItem(this Chat chat, Guid userId)
    {
        var lastMessage = chat.Messages.FirstOrDefault();

        var lastMessageDto = new LastChatMessagePreview(lastMessage?.Sender.Username ?? string.Empty,
            lastMessage?.Content ?? string.Empty, lastMessage?.Attachment is not null, lastMessage?.CreatedAt ?? default);

        var chatPreview = new UserChatListItem(userId, chat.Id, chat.Name, chat.ChatPicture?.Url ?? string.Empty, lastMessageDto);

        return chatPreview;
    }
}