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
    [SwaggerSchema(Description = "URL аватара друга (может быть null)")]
    string? AvatarUrl,
    [SwaggerSchema(Description = "Дата и время последнего посещения")]
    DateTime LastSeen
);