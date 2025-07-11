using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Friends;

/// <summary>
/// DTO друга
/// </summary>
[SwaggerSchema(Description = "Информация о друге")]
public record FriendDto(
    [SwaggerSchema(Description = "Уникальный идентификатор друга")]
    Guid Id,
    [SwaggerSchema(Description = "Имя пользователя друга")]
    string Username,
    [SwaggerSchema(Description = "Персональная ссылка профиля (slug)")]
    string Slug,
    [SwaggerSchema(Description = "URL аватара друга (может быть null)")]
    string? AvatarUrl,
    [SwaggerSchema(Description = "Дата и время последнего посещения")]
    DateTime LastSeen,
    [SwaggerSchema(Description = "Заблокирован ли этот друг текущим пользователем")]
    bool IsBlockedByMe,
    [SwaggerSchema(Description = "Заблокировал ли этот друг текущего пользователя")]
    bool HasBlockedMe,
    [SwaggerSchema(Description = "ID чата между пользователями (может быть null если чата нет)")]
    Guid? ChatId,
    [SwaggerSchema(Description = "Отключен ли чат с этим пользователем")]
    bool IsChatDisabled,
    [SwaggerSchema(Description = "Отключены ли уведомления от этого чата")]
    bool IsChatMuted
);