using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Responses.Profile;

[SwaggerSchema(Description = "Настройки уведомлений пользователя")]
public record NotificationSettingsDto(
    [SwaggerSchema(Description = "ID пользователя")]
    Guid UserId,
    
    [SwaggerSchema(Description = "Общие настройки уведомлений")]
    bool IsEnabled,
    
    [SwaggerSchema(Description = "Уведомления через Telegram")]
    bool IsTelegramEnabled,
    
    [SwaggerSchema(Description = "Уведомления по email")]
    bool IsEmailEnabled,
    
    [SwaggerSchema(Description = "Push-уведомления")]
    bool IsPushEnabled,
    
    [SwaggerSchema(Description = "Новые посты турниров")]
    bool NewTournamentPosts,
    
    [SwaggerSchema(Description = "Обновления турниров")]
    bool TournamentUpdates,
    
    [SwaggerSchema(Description = "Приглашения в команды турниров")]
    bool TournamentTeamInvites,
    
    [SwaggerSchema(Description = "Изменения в команде")]
    bool TeamChanged,
    
    [SwaggerSchema(Description = "Приглашения в команды")]
    bool TeamInvites,
    
    [SwaggerSchema(Description = "Приглашения в турниры")]
    bool TournamentInvites,
    
    [SwaggerSchema(Description = "Назначение роли администратора")]
    bool AdminRoleAssigned,
    
    [SwaggerSchema(Description = "Приглашения в лобби турниров")]
    bool TournamentLobbyInvites,
    
    [SwaggerSchema(Description = "Новые этапы турниров")]
    bool NewTournamentStep,
    
    [SwaggerSchema(Description = "Начало турнира")]
    bool TournamentStarted,
    
    [SwaggerSchema(Description = "Новые запросы в друзья")]
    bool NewFriendRequest,
    
    [SwaggerSchema(Description = "Новые сообщения")]
    bool NewMessage,
    
    [SwaggerSchema(Description = "Новые комментарии к постам")]
    bool NewPostComment,
    
    [SwaggerSchema(Description = "Ответы на комментарии")]
    bool NewCommentReply,
    
    [SwaggerSchema(Description = "Реакции на комментарии")]
    bool NewCommentReaction
); 