using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Profile;

/// <summary>
/// DTO привязанного аккаунта пользователя
/// </summary>
[SwaggerSchema(Description = "Информация о привязанном аккаунте пользователя")]
public record LinkedAccountDto(
    [SwaggerSchema(Description = "Тип аккаунта (Steam, Discord, Telegram)")]
    string Type,
    [SwaggerSchema(Description = "Идентификатор аккаунта (SteamID, Discord ID, Telegram ID)")]
    string LinkedData,
    [SwaggerSchema(Description = "Отображаемое имя типа аккаунта")]
    string DisplayName
);

/// <summary>
/// Типы привязанных аккаунтов
/// </summary>
public static class LinkedAccountTypes
{
    public const string Steam = "Steam";
    public const string Discord = "Discord";
    public const string Telegram = "Telegram";
} 