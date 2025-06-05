namespace Application.Dtos.Responses.Chats;

public record UserChatListItem(Guid UserId, Guid ChatId, string ChatName, string? photoUrl, LastChatMessagePreview LastMessagePreview);

public record LastChatMessagePreview(string Username, string Text, bool HasAttachment, DateTime CreatedAt);