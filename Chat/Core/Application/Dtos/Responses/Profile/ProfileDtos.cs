using Domain.Models.Users;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Profile;

/// <summary>
/// DTO профиля пользователя
/// </summary>
[SwaggerSchema(Description = "Информация о профиле пользователя")]
public record ProfileDto(
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
    string? AvatarUrl, 
    
    /// <summary>
    /// URL обложки профиля
    /// </summary>
    [SwaggerSchema(Description = "URL обложки профиля (может быть null)")]
    string? CoverUrl, 
    
    /// <summary>
    /// Настройки приватности
    /// </summary>
    [SwaggerSchema(Description = "Настройки приватности профиля")]
    PrivacySettingsEnums PrivacySetting);

/// <summary>
/// DTO для валидации имени пользователя
/// </summary>
[SwaggerSchema(Description = "Результат валидации имени пользователя")]
public record ProfileValidationDto(
    /// <summary>
    /// Доступность имени пользователя
    /// </summary>
    [SwaggerSchema(Description = "Доступно ли имя пользователя")]
    bool IsAvailable, 
    
    /// <summary>
    /// Сообщение об ошибке
    /// </summary>
    [SwaggerSchema(Description = "Сообщение об ошибке (если есть)")]
    string? ErrorMessage); 