using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.BlackList;

/// <summary>
/// DTO элемента черного списка
/// </summary>
[SwaggerSchema(Description = "Информация о заблокированном пользователе")]
public record BlackListItemDto(
    [SwaggerSchema(Description = "Уникальный идентификатор заблокированного пользователя")]
    Guid Id,
    [SwaggerSchema(Description = "Имя заблокированного пользователя")]
    string Username,
    [SwaggerSchema(Description = "Персональная ссылка профиля (slug)")]
    string Slug,
    [SwaggerSchema(Description = "Email заблокированного пользователя")]
    string Email,
    [SwaggerSchema(Description = "URL аватара заблокированного пользователя (может быть null)")]
    string? AvatarUrl);