using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos;

/// <summary>
/// DTO медиа файла
/// </summary>
[SwaggerSchema(Description = "Информация о медиа файле")]
public record MediaDto(
    [SwaggerSchema(Description = "Уникальный идентификатор медиа файла")]
    Guid Id,
    [SwaggerSchema(Description = "Название медиа файла")]
    string Name,
    [SwaggerSchema(Description = "URL для доступа к медиа файлу")]
    string Url,
    [SwaggerSchema(Description = "Описание медиа файла (может быть null)")]
    string? Description);