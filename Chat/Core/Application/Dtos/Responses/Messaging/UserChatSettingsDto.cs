using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Messaging;

[SwaggerSchema(Description = "Настройки чата пользователя")]
public record UserChatSettingsDto(
    [SwaggerSchema(Description = "ID настроек")]
    Guid Id,
    
    [SwaggerSchema(Description = "ID пользователя")]
    Guid UserId,
    
    [SwaggerSchema(Description = "ID чата")]
    Guid ChatId,
    
    [SwaggerSchema(Description = "Чат заглушен")]
    bool IsMuted,
    
    [SwaggerSchema(Description = "Чат отключен")]
    bool IsDisabled,
    
    [SwaggerSchema(Description = "Список отключенных пользователей")]
    List<Guid> DisabledUserIds
); 