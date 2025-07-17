using Application.Dtos.Responses.Chats;
using Domain.Models.Messaging;
using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;

namespace Application.Common.Mappers;

public static class Chats
{
    public static async Task<UserChatListItemDto> ToUserChatListItemDtoAsync(this ChatWithUnreadCount chatWithUnreadCount, Guid userId, IFilesSigningService filesSigningService, bool isMuted = false, CancellationToken cancellationToken = default)
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
                lastMessage.Attachment != null,
                lastMessage.CreatedAt
            );
        }

        string? imageUrl = null;
        if (!string.IsNullOrEmpty(chat.ChatPicture?.Url))
        {
            imageUrl = await filesSigningService.GetSignedUrlForObjectAsync(chat.ChatPicture.Url, "neva-avatars", cancellationToken);
        }

        var displayName = chat.Name;
        if (!isGroup && chat.Users.Count == 2)
        {
            var interlocutor = chat.Users.First(u => u.Id != userId);
            displayName = interlocutor.Username;
            
            if (!string.IsNullOrEmpty(interlocutor.Avatar?.Url))
            {
                imageUrl = await filesSigningService.GetSignedUrlForObjectAsync(interlocutor.Avatar.Url, "neva-avatars", cancellationToken);
            }
        }

        return new UserChatListItemDto(
            chat.Id,
            displayName,
            imageUrl,
            unreadCount,
            lastMessagePreview,
            userRole,
            isGroup,
            isMuted
        );
    }
}