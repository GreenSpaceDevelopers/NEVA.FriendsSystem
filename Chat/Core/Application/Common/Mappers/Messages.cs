using Application.Abstractions.Services.Communications.Data;
using Application.Dtos.Messaging;
using Application.Messaging.Proto.Messages;

namespace Application.Common.Mappers;

public static class Messages
{
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

    public static object MessageToSend(MessageToRoute messageToRoute)
    {
        return new object();
    }

    public static MessageToRoute Unverified(this RawMessage message) => new(message.ConnectionId!, Status: Status.Unverified);
    public static MessageToRoute Unauthorized(this RawMessage message) => new(message.ConnectionId!, Status: Status.Unauthorized);
}