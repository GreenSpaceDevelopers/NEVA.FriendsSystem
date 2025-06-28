using Application.Messaging.Proto.Messages;

namespace Application.Dtos.Messaging;

public record RawMessage(
    Guid MessageId,
    RequestType MessageType,
    MessageType? Type,
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