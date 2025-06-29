using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Chats;

/// <summary>
/// DTO элемента списка чатов пользователя
/// </summary>
[SwaggerSchema(Description = "Информация о чате в списке пользователя")]
public record UserChatListItemDto(
    /// <summary>
    /// Уникальный идентификатор чата
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор чата")]
    Guid Id,
    
    /// <summary>
    /// Название чата
    /// </summary>
    [SwaggerSchema(Description = "Название чата")]
    string Name,
    
    /// <summary>
    /// URL изображения чата
    /// </summary>
    [SwaggerSchema(Description = "URL изображения чата (может быть null)")]
    string? ImageUrl,
    
    /// <summary>
    /// Количество непрочитанных сообщений
    /// </summary>
    [SwaggerSchema(Description = "Количество непрочитанных сообщений в чате")]
    int UnreadCount,
    
    /// <summary>
    /// Последнее сообщение в чате
    /// </summary>
    [SwaggerSchema(Description = "Текст последнего сообщения в чате (может быть null)")]
    string? LastMessage,
    
    /// <summary>
    /// Время последнего сообщения
    /// </summary>
    [SwaggerSchema(Description = "Дата и время последнего сообщения (может быть null)")]
    DateTime? LastMessageTime,
    
    /// <summary>
    /// Роль пользователя в чате
    /// </summary>
    [SwaggerSchema(Description = "Роль текущего пользователя в чате")]
    string UserRole);

public record UserChatListItem(Guid UserId, Guid ChatId, string ChatName, string? photoUrl, LastChatMessagePreview LastMessagePreview);

public record LastChatMessagePreview(string Username, string Text, bool HasAttachment, DateTime CreatedAt);