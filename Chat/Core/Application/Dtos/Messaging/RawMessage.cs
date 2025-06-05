using Application.Messaging.Proto.Messages;
using Type = Application.Messaging.Proto.Messages.Type;

namespace Application.Dtos.Messaging;

public record RawMessage(
    Guid MessageId,
    RequestType MessageType,
    Type? Type,
    string? ConnectionId,
    string? AccessToken,
    string? Message,
    string? StickerId,
    string? ReactionId,
    string? ChatId,
    string Hash);

public enum RequestType
{
    Message,
    ConnectionRequest
}