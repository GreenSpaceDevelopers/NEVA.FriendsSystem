using Domain.Models.Users;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Profile;

/// <summary>
/// DTO настроек приватности пользователя
/// </summary>
[SwaggerSchema(Description = "Настройки приватности пользователя")]
public record UserPrivacySettingsDto(
    Guid Id,
    [SwaggerSchema(Description = "Кто может видеть список друзей (0=Private, 1=Friends, 2=Public)")]
    PrivacyLevel FriendsListVisibility,
    [SwaggerSchema(Description = "Кто может оставлять комментарии (0=Private, 1=Friends, 2=Public)")]
    PrivacyLevel CommentsPermission,
    [SwaggerSchema(Description = "Кто может отправлять личные сообщения (0=Private, 1=Friends, 2=Public)")]
    PrivacyLevel DirectMessagesPermission
);

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
    [SwaggerSchema(Description = "Настройки приватности пользователя")]
    UserPrivacySettingsDto PrivacySettings,
    [SwaggerSchema(Description = "Заблокирован ли этот пользователь текущим пользователем")]
    bool IsBlockedByMe,
    [SwaggerSchema(Description = "Заблокировал ли этот пользователь текущего пользователя")]
    bool HasBlockedMe
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