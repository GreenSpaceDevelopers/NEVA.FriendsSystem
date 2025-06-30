using Swashbuckle.AspNetCore.Annotations;

namespace Application.Dtos.Requests.Profile;

[SwaggerSchema(Description = "Запрос на обновление настроек уведомлений")]
public record UpdateNotificationSettingsRequest(
    [SwaggerSchema(Description = "ID пользователя")]
    Guid UserId,
    
    [SwaggerSchema(Description = "Общие настройки уведомлений")]
    bool? IsEnabled = null,
    
    [SwaggerSchema(Description = "Уведомления через Telegram")]
    bool? IsTelegramEnabled = null,
    
    [SwaggerSchema(Description = "Уведомления по email")]
    bool? IsEmailEnabled = null,
    
    [SwaggerSchema(Description = "Push-уведомления")]
    bool? IsPushEnabled = null,
    
    [SwaggerSchema(Description = "Новые посты турниров")]
    bool? NewTournamentPosts = null,
    
    [SwaggerSchema(Description = "Обновления турниров")]
    bool? TournamentUpdates = null,
    
    [SwaggerSchema(Description = "Приглашения в команды турниров")]
    bool? TournamentTeamInvites = null,
    
    [SwaggerSchema(Description = "Изменения в команде")]
    bool? TeamChanged = null,
    
    [SwaggerSchema(Description = "Приглашения в команды")]
    bool? TeamInvites = null,
    
    [SwaggerSchema(Description = "Приглашения в турниры")]
    bool? TournamentInvites = null,
    
    [SwaggerSchema(Description = "Назначение роли администратора")]
    bool? AdminRoleAssigned = null,
    
    [SwaggerSchema(Description = "Приглашения в лобби турниров")]
    bool? TournamentLobbyInvites = null,
    
    [SwaggerSchema(Description = "Новые этапы турниров")]
    bool? NewTournamentStep = null,
    
    [SwaggerSchema(Description = "Начало турнира")]
    bool? TournamentStarted = null,
    
    [SwaggerSchema(Description = "Новые запросы в друзья")]
    bool? NewFriendRequest = null,
    
    [SwaggerSchema(Description = "Новые сообщения")]
    bool? NewMessage = null,
    
    [SwaggerSchema(Description = "Новые комментарии к постам")]
    bool? NewPostComment = null,
    
    [SwaggerSchema(Description = "Ответы на комментарии")]
    bool? NewCommentReply = null,
    
    [SwaggerSchema(Description = "Реакции на комментарии")]
    bool? NewCommentReaction = null
); 