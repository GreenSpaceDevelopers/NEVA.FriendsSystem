using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Friends;

/// <summary>
/// DTO заявки в друзья
/// </summary>
[SwaggerSchema(Description = "Информация о заявке в друзья")]
public record FriendRequestsDto(
    [SwaggerSchema(Description = "Уникальный идентификатор отправителя")]
    Guid SenderId,
    [SwaggerSchema(Description = "Уникальный идентификатор получателя")]
    Guid ReceiverId,
    [SwaggerSchema(Description = "Имя пользователя")]
    string Username,
    [SwaggerSchema(Description = "URL аватара пользователя (может быть null)")]
    string? AvatarUrl
);