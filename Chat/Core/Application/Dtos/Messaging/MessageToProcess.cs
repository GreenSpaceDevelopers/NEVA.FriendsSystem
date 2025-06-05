using Type = Application.Messaging.Proto.Messages.Type;

namespace Application.Dtos.Messaging;

public record MessageToProcess(string UserId, Guid messageMessageId, Type? MessageType, string? Message, string? MessageStickerId, string? MessageReactionId, string ChatId);