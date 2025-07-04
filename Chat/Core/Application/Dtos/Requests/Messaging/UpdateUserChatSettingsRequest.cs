using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Requests.Messaging;

[SwaggerSchema(Description = "Запрос на обновление настроек чата пользователя")]
public record UpdateUserChatSettingsRequest (
    [SwaggerSchema(Description = "ID пользователя")]
    Guid UserId,
    
    [SwaggerSchema(Description = "ID чата")]
    Guid ChatId,
    
    [SwaggerSchema(Description = "Чат заглушен")]
    bool? IsMuted = null,
    
    [SwaggerSchema(Description = "Чат отключен")]
    bool? IsDisabled = null,
    
    [SwaggerSchema(Description = "Список ID отключенных пользователей")]
    List<Guid>? DisabledUserIds = null
) : IRequest; 