using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Common.Models;

/// <summary>
/// Модель ошибки API
/// </summary>
[SwaggerSchema(Description = "Информация об ошибке")]
public record ErrorResponse(
    [SwaggerSchema(Description = "Код ошибки")]
    string Code,
    [SwaggerSchema(Description = "Сообщение об ошибке")]
    string Message,
    [SwaggerSchema(Description = "Дополнительные детали ошибки")]
    object? Details = null);

/// <summary>
/// Модель ошибки валидации
/// </summary>
[SwaggerSchema(Description = "Ошибка валидации данных")]
public record ValidationErrorResponse(
    [SwaggerSchema(Description = "Код ошибки")]
    string Code,
    [SwaggerSchema(Description = "Сообщение об ошибке")]
    string Message,
    [SwaggerSchema(Description = "Ошибки валидации по полям")]
    Dictionary<string, string[]> FieldErrors);

/// <summary>
/// Модель ошибки "не найдено"
/// </summary>
[SwaggerSchema(Description = "Ресурс не найден")]
public record NotFoundErrorResponse(
    [SwaggerSchema(Description = "Код ошибки")]
    string Code,
    [SwaggerSchema(Description = "Сообщение об ошибке")]
    string Message,
    [SwaggerSchema(Description = "Тип ресурса, который не найден")]
    string ResourceType,
    [SwaggerSchema(Description = "ID ресурса, который не найден")]
    string ResourceId);