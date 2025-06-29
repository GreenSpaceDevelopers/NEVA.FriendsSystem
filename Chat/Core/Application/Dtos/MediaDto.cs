using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos;

/// <summary>
/// DTO медиа файла
/// </summary>
[SwaggerSchema(Description = "Информация о медиа файле")]
public record MediaDto(
    /// <summary>
    /// Уникальный идентификатор медиа файла
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор медиа файла")]
    Guid Id,
    
    /// <summary>
    /// Название медиа файла
    /// </summary>
    [SwaggerSchema(Description = "Название медиа файла")]
    string Name,
    
    /// <summary>
    /// URL медиа файла
    /// </summary>
    [SwaggerSchema(Description = "URL для доступа к медиа файлу")]
    string Url,
    
    /// <summary>
    /// Описание медиа файла
    /// </summary>
    [SwaggerSchema(Description = "Описание медиа файла (может быть null)")]
    string? Description);