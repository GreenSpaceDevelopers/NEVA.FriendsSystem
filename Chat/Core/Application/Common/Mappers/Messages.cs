using Application.Dtos.Messaging;
using Application.Dtos.Responses.Chats;
using Application.Messaging.Proto.Messages;
using Domain.Models.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;

namespace Application.Common.Mappers;

public static class Messages
{
    public static MessageDto ToMessageDto(this Message message)
    {
        return new MessageDto(
            message.Id,
            message.ChatId,
            message.SenderId,
            message.Sender.Username,
            message.Sender.PersonalLink,
            message.Sender.Avatar?.Url,
            message.Content,
            message.Attachment?.Url,
            message.CreatedAt,
            message.Replies.Count,
            message.Reactions.Count
        );
    }

    public static async Task<MessageDto> ToMessageDtoAsync(this Message message, IFilesSigningService filesSigningService, CancellationToken cancellationToken = default)
    {
        string? avatarUrl;
        if (!string.IsNullOrEmpty(message.Sender.Avatar?.Url))
        {
            avatarUrl = await filesSigningService.GetSignedUrlForObjectAsync(message.Sender.Avatar.Url, message.Sender.Avatar.BucketName ?? "neva-avatars", cancellationToken);
        }
        else
        {
            avatarUrl = "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png";
        }

        string? attachmentUrl = null;
        if (!string.IsNullOrEmpty(message.Attachment?.Url))
        {
            attachmentUrl = await filesSigningService.GetSignedUrlAsync(message.Attachment.Url, cancellationToken);
        }

        return new MessageDto(
            message.Id,
            message.ChatId,
            message.SenderId,
            message.Sender.Username,
            message.Sender.PersonalLink,
            avatarUrl,
            message.Content,
            attachmentUrl,
            message.CreatedAt,
            message.Replies.Count,
            message.Reactions.Count
        );
    }

    public static RawMessage ToRawMessage(this ReceivedMessage receivedMessage)
    {
        return new RawMessage(
            Guid.Parse(receivedMessage.OptionalMessageId ?? string.Empty),
            RequestType.Message,
            receivedMessage.Type,
            receivedMessage.OptionalConnectionId,
            receivedMessage.AccessToken,
            receivedMessage.OptionalMessage,
            receivedMessage.OptionalStickerId,
            receivedMessage.OptionalReactionId,
            receivedMessage.ChatId,
            receivedMessage.Hash);
    }

    public static RawMessage ToRawMessage(this ConnectionRequest connectionRequestMessage)
    {
        return new RawMessage(
            Guid.NewGuid(),
            RequestType.ConnectionRequest,
            null,
            null,
            connectionRequestMessage.AccessToken,
            null,
            null,
            null,
            null,
            connectionRequestMessage.Hash);
    }

    public static object MessageToSend(MessageToRoute? messageToRoute)
    {
        return new object();
    }

    public static MessageToRoute Unverified(this RawMessage message) => new(message.ConnectionId!, Status: Status.Unverified);

    public static MessageToRoute Unauthorized(this RawMessage message) => new(message.ConnectionId!, Status: Status.Unauthorized);
}