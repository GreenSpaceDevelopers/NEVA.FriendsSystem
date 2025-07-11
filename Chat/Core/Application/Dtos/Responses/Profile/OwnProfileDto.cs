using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Profile;

[SwaggerSchema(Description = "Информация о собственном профиле пользователя")] 
public record OwnProfileDto(
    [property: SwaggerSchema(Description = "Уникальный идентификатор пользователя")] Guid Id,
    [property: SwaggerSchema(Description = "Имя пользователя")] string Username,
    [property: SwaggerSchema(Description = "Персональная ссылка профиля (slug)")] string Slug,
    [property: SwaggerSchema(Description = "Email пользователя")] string Email,
    [property: SwaggerSchema(Description = "Имя")] string? Name,
    [property: SwaggerSchema(Description = "Фамилия")] string? Surname,
    [property: SwaggerSchema(Description = "Отчество")] string? MiddleName,
    [property: SwaggerSchema(Description = "Дата рождения")] DateTime? DateOfBirth,
    [property: SwaggerSchema(Description = "URL аватара пользователя (может быть null)")] string? AvatarUrl,
    [property: SwaggerSchema(Description = "URL обложки профиля (может быть null)")] string? CoverUrl,
    [property: SwaggerSchema(Description = "Настройки приватности пользователя")] UserPrivacySettingsDto PrivacySettings,
    [property: SwaggerSchema(Description = "Есть ли непрочтенные сообщения")] bool HasUnreadMessages,
    [property: SwaggerSchema(Description = "Есть ли непринятые заявки в друзья")] bool HasPendingFriendRequests,
    [property: SwaggerSchema(Description = "Список привязанных аккаунтов (Steam, Discord, Telegram)")] IReadOnlyList<LinkedAccountDto> LinkedAccounts
); 