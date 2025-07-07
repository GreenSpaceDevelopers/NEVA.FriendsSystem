using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Chats;

/// <summary>
/// DTO элемента списка чатов пользователя
/// </summary>
[SwaggerSchema(Description = "Информация о чате в списке пользователя")]
public record UserChatListItemDto(
    [SwaggerSchema(Description = "Уникальный идентификатор чата")]
    Guid Id,
    [SwaggerSchema(Description = "Название чата")]
    string Name,
    [SwaggerSchema(Description = "URL изображения чата (может быть null)")]
    string? ImageUrl,
    [SwaggerSchema(Description = "Количество непрочитанных сообщений в чате")]
    int UnreadCount,
    [SwaggerSchema(Description = "Текст последнего сообщения в чате (может быть null)")]
    string? LastMessage,
    [SwaggerSchema(Description = "Дата и время последнего сообщения (может быть null)")]
    DateTime? LastMessageTime,
    [SwaggerSchema(Description = "Роль текущего пользователя в чате")]
    string UserRole,
    [SwaggerSchema(Description = "Является ли чат групповым")]
    bool IsGroup);

public record UserChatListItem(Guid UserId, Guid ChatId, string ChatName, string? photoUrl, LastChatMessagePreview LastMessagePreview, bool IsGroup);

public record LastChatMessagePreview(string Username, string Text, bool HasAttachment, DateTime CreatedAt);