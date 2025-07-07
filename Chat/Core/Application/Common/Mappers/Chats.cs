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

        var isGroup = chat.Users.Count > 2;

        var displayName = chat.Name;
        if (!isGroup && chat.Users.Count == 2)
        {
            var interlocutor = chat.Users.First(u => u.Id != userId);
            displayName = interlocutor.Username;
        }

        var chatPreview = new UserChatListItem(userId, chat.Id, displayName, chat.ChatPicture?.Url ?? string.Empty, lastMessageDto, isGroup);

        return chatPreview;
    }
}