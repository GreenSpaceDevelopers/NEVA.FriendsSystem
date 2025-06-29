using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Common.Models;

/// <summary>
/// Модель ошибки API
/// </summary>
[SwaggerSchema(Description = "Информация об ошибке")]
public record ErrorResponse(
    /// <summary>
    /// Код ошибки
    /// </summary>
    [SwaggerSchema(Description = "Код ошибки")]
    string Code,
    
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    [SwaggerSchema(Description = "Сообщение об ошибке")]
    string Message,
    
    /// <summary>
    /// Детали ошибки
    /// </summary>
    [SwaggerSchema(Description = "Дополнительные детали ошибки")]
    object? Details = null);

/// <summary>
/// Модель ошибки валидации
/// </summary>
[SwaggerSchema(Description = "Ошибка валидации данных")]
public record ValidationErrorResponse(
    /// <summary>
    /// Код ошибки
    /// </summary>
    [SwaggerSchema(Description = "Код ошибки")]
    string Code,
    
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    [SwaggerSchema(Description = "Сообщение об ошибке")]
    string Message,
    
    /// <summary>
    /// Ошибки валидации по полям
    /// </summary>
    [SwaggerSchema(Description = "Ошибки валидации по полям")]
    Dictionary<string, string[]> FieldErrors);

/// <summary>
/// Модель ошибки "не найдено"
/// </summary>
[SwaggerSchema(Description = "Ресурс не найден")]
public record NotFoundErrorResponse(
    /// <summary>
    /// Код ошибки
    /// </summary>
    [SwaggerSchema(Description = "Код ошибки")]
    string Code,
    
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    [SwaggerSchema(Description = "Сообщение об ошибке")]
    string Message,
    
    /// <summary>
    /// Тип ресурса
    /// </summary>
    [SwaggerSchema(Description = "Тип ресурса, который не найден")]
    string ResourceType,
    
    /// <summary>
    /// ID ресурса
    /// </summary>
    [SwaggerSchema(Description = "ID ресурса, который не найден")]
    string ResourceId); 