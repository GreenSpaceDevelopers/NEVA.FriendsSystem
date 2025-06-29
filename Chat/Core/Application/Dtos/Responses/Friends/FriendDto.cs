using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Friends;

/// <summary>
/// DTO друга
/// </summary>
[SwaggerSchema(Description = "Информация о друге")]
public record FriendDto(
    /// <summary>
    /// Уникальный идентификатор друга
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор друга")]
    Guid Id,
    
    /// <summary>
    /// Имя друга
    /// </summary>
    [SwaggerSchema(Description = "Имя пользователя друга")]
    string Username,
    
    /// <summary>
    /// URL аватара друга
    /// </summary>
    [SwaggerSchema(Description = "URL аватара друга (может быть null)")]
    string? AvatarUrl,
    
    /// <summary>
    /// Время последнего посещения
    /// </summary>
    [SwaggerSchema(Description = "Дата и время последнего посещения")]
    DateTime LastSeen
); 