using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace WebApi.Common.Models;

/// <summary>
/// Общие параметры пагинации
/// </summary>
[SwaggerSchema(Description = "Параметры пагинации")]
public record PaginationParameters(
    [SwaggerSchema(Description = "Номер страницы (начиная с 1)")]
    [Range(1, int.MaxValue)]
    ushort Page,
    [SwaggerSchema(Description = "Размер страницы (от 1 до 100)")]
    [Range(1, 100)]
    ushort Size);

/// <summary>
/// Параметры поиска
/// </summary>
[SwaggerSchema(Description = "Параметры поиска")]
public record SearchParameters(
    [SwaggerSchema(Description = "Строка для поиска (минимум 1 символ)")]
    [MinLength(1)]
    string Query,

    [SwaggerSchema(Description = "Номер страницы (начиная с 1)")]
    [Range(1, int.MaxValue)]
    ushort PageNumber,

    [SwaggerSchema(Description = "Размер страницы (от 1 до 100)")]
    [Range(1, 100)]
    ushort PageSize);

/// <summary>
/// Параметры сортировки
/// </summary>
[SwaggerSchema(Description = "Параметры сортировки")]
public record SortParameters(
    [SwaggerSchema(Description = "Поле для сортировки")]
    string SortBy,
    [SwaggerSchema(Description = "Направление сортировки: asc (по возрастанию) или desc (по убыванию)")]
    string SortDirection = "desc");

/// <summary>
/// Параметры фильтрации
/// </summary>
[SwaggerSchema(Description = "Параметры фильтрации")]
public record FilterParameters(
    [SwaggerSchema(Description = "Дата начала периода (формат: yyyy-MM-dd)")]
    DateTime? FromDate,
    [SwaggerSchema(Description = "Дата окончания периода (формат: yyyy-MM-dd)")]
    DateTime? ToDate,
    [SwaggerSchema(Description = "Статус для фильтрации")]
    string? Status);