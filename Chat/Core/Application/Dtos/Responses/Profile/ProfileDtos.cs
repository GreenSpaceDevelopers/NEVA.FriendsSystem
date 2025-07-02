using Domain.Models.Users;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Profile;

/// <summary>
/// DTO профиля пользователя
/// </summary>
public class PrivacySettingDto
{
    public int Id { get; set; }
    public Guid ChatUserId { get; set; }
    public string SettingName { get; set; } = null!;
}

[SwaggerSchema(Description = "Информация о профиле пользователя")]
public record ProfileDto(
    [SwaggerSchema(Description = "Уникальный идентификатор пользователя")]
    Guid Id,
    [SwaggerSchema(Description = "Имя пользователя")]
    string Username,
    [SwaggerSchema(Description = "Имя")]
    string? Name,
    [SwaggerSchema(Description = "Фамилия")]
    string? Surname,
    [SwaggerSchema(Description = "Отчество")]
    string? MiddleName,
    [SwaggerSchema(Description = "Дата рождения")]
    DateTime? DateOfBirth,
    [SwaggerSchema(Description = "URL аватара пользователя (может быть null)")]
    string? AvatarUrl,
    [SwaggerSchema(Description = "URL обложки профиля (может быть null)")]
    string? CoverUrl,
    [SwaggerSchema(Description = "Настройки приватности профиля (расширенный объект)")]
    PrivacySettingDto PrivacySetting
);

/// <summary>
/// DTO для валидации имени пользователя
/// </summary>
[SwaggerSchema(Description = "Результат валидации имени пользователя")]
public record ProfileValidationDto(
    [SwaggerSchema(Description = "Доступно ли имя пользователя")]
    bool IsAvailable,
    [SwaggerSchema(Description = "Сообщение об ошибке (если есть)")]
    string? ErrorMessage);