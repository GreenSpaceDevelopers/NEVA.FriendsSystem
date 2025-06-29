using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Common.Models;

/// <summary>
/// Общие параметры пагинации
/// </summary>
[SwaggerSchema(Description = "Параметры пагинации")]
public record PaginationParameters(
    /// <summary>
    /// Номер страницы
    /// </summary>
    [SwaggerSchema(Description = "Номер страницы (начиная с 1)", Minimum = 1)]
    ushort Page,
    
    /// <summary>
    /// Размер страницы
    /// </summary>
    [SwaggerSchema(Description = "Размер страницы (от 1 до 100)", Minimum = 1, Maximum = 100)]
    ushort Size);

/// <summary>
    /// Параметры поиска
    /// </summary>
[SwaggerSchema(Description = "Параметры поиска")]
public record SearchParameters(
    /// <summary>
    /// Строка поиска
    /// </summary>
    [SwaggerSchema(Description = "Строка для поиска (минимум 1 символ)")]
    string Query,
    
    /// <summary>
    /// Номер страницы
    /// </summary>
    [SwaggerSchema(Description = "Номер страницы (начиная с 1)", Minimum = 1)]
    ushort PageNumber,
    
    /// <summary>
    /// Размер страницы
    /// </summary>
    [SwaggerSchema(Description = "Размер страницы (от 1 до 100)", Minimum = 1, Maximum = 100)]
    ushort PageSize);

/// <summary>
/// Параметры сортировки
/// </summary>
[SwaggerSchema(Description = "Параметры сортировки")]
public record SortParameters(
    /// <summary>
    /// Поле для сортировки
    /// </summary>
    [SwaggerSchema(Description = "Поле для сортировки")]
    string SortBy,
    
    /// <summary>
    /// Направление сортировки
    /// </summary>
    [SwaggerSchema(Description = "Направление сортировки: asc (по возрастанию) или desc (по убыванию)")]
    string SortDirection = "desc");

/// <summary>
/// Параметры фильтрации
/// </summary>
[SwaggerSchema(Description = "Параметры фильтрации")]
public record FilterParameters(
    /// <summary>
    /// Дата начала периода
    /// </summary>
    [SwaggerSchema(Description = "Дата начала периода (формат: yyyy-MM-dd)")]
    DateTime? FromDate,
    
    /// <summary>
    /// Дата окончания периода
    /// </summary>
    [SwaggerSchema(Description = "Дата окончания периода (формат: yyyy-MM-dd)")]
    DateTime? ToDate,
    
    /// <summary>
    /// Статус
    /// </summary>
    [SwaggerSchema(Description = "Статус для фильтрации")]
    string? Status); 