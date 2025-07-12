using Application.Dtos.Responses.Chats;
using Domain.Models.Messaging;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;

namespace Application.Common.Mappers;

public static class Chats
{
    public static UserChatListItem ToUserChatListItem(this Chat chat, Guid userId)
    {
        var lastMessage = chat.Messages.FirstOrDefault();

        var lastMessageDto = new LastChatMessagePreview(lastMessage?.Sender.Username ?? string.Empty,
            lastMessage?.Content ?? string.Empty, lastMessage?.Attachments?.Count > 0, lastMessage?.CreatedAt ?? default);

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

    public static UserChatListItemDto ToUserChatListItemDto(this ChatWithUnreadCount chatWithUnreadCount, Guid userId)
    {
        var chat = chatWithUnreadCount.Chat;
        var unreadCount = chatWithUnreadCount.UnreadCount;

        var lastMessage = chat.Messages.FirstOrDefault();
        var isGroup = chat.Users.Count > 2;
        var userRole = chat.AdminId == userId ? "Creator" : "Member";

        LastChatMessagePreview? lastMessagePreview = null;
        if (lastMessage != null)
        {
            lastMessagePreview = new LastChatMessagePreview(
                lastMessage.Sender?.Username ?? string.Empty,
                lastMessage.Content ?? string.Empty,
                lastMessage.Attachments?.Count > 0,
                lastMessage.CreatedAt
            );
        }

        var displayName = chat.Name;
        if (isGroup || chat.Users.Count != 2)
            return new UserChatListItemDto(
                chat.Id,
                displayName,
                chat.ChatPicture?.Url,
                unreadCount,
                lastMessagePreview,
                userRole,
                isGroup
            );
        
        var interlocutor = chat.Users.First(u => u.Id != userId);
        displayName = interlocutor.Username;

        return new UserChatListItemDto(
            chat.Id,
            displayName,
            chat.ChatPicture?.Url,
            unreadCount,
            lastMessagePreview,
            userRole,
            isGroup
        );
    }

    public static async Task<UserChatListItemDto> ToUserChatListItemDtoAsync(this ChatWithUnreadCount chatWithUnreadCount, Guid userId, IFilesSigningService filesSigningService, CancellationToken cancellationToken = default)
    {
        var chat = chatWithUnreadCount.Chat;
        var unreadCount = chatWithUnreadCount.UnreadCount;

        var lastMessage = chat.Messages.FirstOrDefault();
        var isGroup = chat.Users.Count > 2;
        var userRole = chat.AdminId == userId ? "Creator" : "Member";

        LastChatMessagePreview? lastMessagePreview = null;
        if (lastMessage != null)
        {
            lastMessagePreview = new LastChatMessagePreview(
                lastMessage.Sender?.Username ?? string.Empty,
                lastMessage.Content ?? string.Empty,
                lastMessage.Attachments?.Count > 0,
                lastMessage.CreatedAt
            );
        }

        string? imageUrl = null;
        if (!string.IsNullOrEmpty(chat.ChatPicture?.Url))
        {
            imageUrl = await filesSigningService.GetSignedUrlAsync(chat.ChatPicture.Url, cancellationToken);
        }

        var displayName = chat.Name;
        if (isGroup || chat.Users.Count != 2)
            return new UserChatListItemDto(
                chat.Id,
                displayName,
                imageUrl,
                unreadCount,
                lastMessagePreview,
                userRole,
                isGroup
            );
        
        var interlocutor = chat.Users.First(u => u.Id != userId);
        displayName = interlocutor.Username;

        return new UserChatListItemDto(
            chat.Id,
            displayName,
            imageUrl,
            unreadCount,
            lastMessagePreview,
            userRole,
            isGroup
        );
    }
}