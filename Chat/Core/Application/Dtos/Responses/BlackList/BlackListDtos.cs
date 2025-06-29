using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.BlackList;

/// <summary>
/// DTO элемента черного списка
/// </summary>
[SwaggerSchema(Description = "Информация о заблокированном пользователе")]
public record BlackListItemDto(
    /// <summary>
    /// Уникальный идентификатор заблокированного пользователя
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор заблокированного пользователя")]
    Guid Id,
    
    /// <summary>
    /// Имя заблокированного пользователя
    /// </summary>
    [SwaggerSchema(Description = "Имя заблокированного пользователя")]
    string Username,
    
    /// <summary>
    /// URL аватара заблокированного пользователя
    /// </summary>
    [SwaggerSchema(Description = "URL аватара заблокированного пользователя (может быть null)")]
    string? AvatarUrl);

/// <summary>
/// DTO результата поиска пользователей
/// </summary>
[SwaggerSchema(Description = "Информация о пользователе в результатах поиска")]
public record UserSearchDto(
    /// <summary>
    /// Уникальный идентификатор пользователя
    /// </summary>
    [SwaggerSchema(Description = "Уникальный идентификатор пользователя")]
    Guid Id,
    
    /// <summary>
    /// Имя пользователя
    /// </summary>
    [SwaggerSchema(Description = "Имя пользователя")]
    string Username,
    
    /// <summary>
    /// URL аватара пользователя
    /// </summary>
    [SwaggerSchema(Description = "URL аватара пользователя (может быть null)")]
    string? AvatarUrl);