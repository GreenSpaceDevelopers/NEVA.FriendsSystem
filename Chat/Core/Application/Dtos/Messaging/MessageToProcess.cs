using Type = Application.Messaging.Proto.Messages;

namespace Application.Dtos.Messaging;

public record MessageToProcess(string UserId, Guid messageMessageId, Type.MessageType? MessageType, string? Message, string? MessageStickerId, string? MessageReactionId, string ChatId);