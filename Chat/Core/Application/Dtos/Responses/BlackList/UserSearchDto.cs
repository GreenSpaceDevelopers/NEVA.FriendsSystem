using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.BlackList;

/// <summary>
/// DTO результата поиска пользователей
/// </summary>
[SwaggerSchema(Description = "Информация о пользователе в результатах поиска")]
public record UserSearchDto(
    [SwaggerSchema(Description = "Уникальный идентификатор пользователя")]
    Guid Id,
    [SwaggerSchema(Description = "Имя пользователя")]
    string Username,
    [SwaggerSchema(Description = "URL аватара пользователя (может быть null)")]
    string? AvatarUrl,
    [SwaggerSchema(Description = "Заблокирован ли этот пользователь текущим пользователем")]
    bool IsBlockedByMe,
    [SwaggerSchema(Description = "Заблокировал ли этот пользователь текущего пользователя")]
    bool HasBlockedMe
);