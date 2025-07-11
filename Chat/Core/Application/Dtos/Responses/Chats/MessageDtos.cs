using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Chats;

/// <summary>
/// DTO сообщения в чате
/// </summary>
[SwaggerSchema(Description = "Информация о сообщении в чате")]
public record MessageDto(
    [SwaggerSchema(Description = "Уникальный идентификатор сообщения")]
    Guid Id,
    [SwaggerSchema(Description = "Уникальный идентификатор чата")]
    Guid ChatId,
    [SwaggerSchema(Description = "Уникальный идентификатор отправителя")]
    Guid SenderId,
    [SwaggerSchema(Description = "Имя пользователя отправителя")]
    string SenderUsername,
    [SwaggerSchema(Description = "Персональная ссылка профиля отправителя (slug)")]
    string SenderSlug,
    [SwaggerSchema(Description = "URL аватара отправителя (может быть null)")]
    string? SenderAvatarUrl,
    [SwaggerSchema(Description = "Содержимое сообщения (может быть null)")]
    string? Content,
    [SwaggerSchema(Description = "URL вложения к сообщению (может быть null)")]
    string? AttachmentUrl,
    [SwaggerSchema(Description = "Дата и время создания сообщения")]
    DateTime CreatedAt,
    [SwaggerSchema(Description = "Количество ответов на сообщение")]
    int RepliesCount,
    [SwaggerSchema(Description = "Количество реакций на сообщение")]
    int ReactionsCount); 