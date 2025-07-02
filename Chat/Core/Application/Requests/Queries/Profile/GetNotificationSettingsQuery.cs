using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Profile;
using Application.Services.ApplicationInfrastructure.Results;
using FluentValidation;

namespace Application.Requests.Queries.Profile;

public record GetNotificationSettingsQuery(Guid UserId) : IRequest;

public class GetNotificationSettingsQueryHandler(INotificationSettingsRepository notificationSettingsRepository) 
    : IRequestHandler<GetNotificationSettingsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetNotificationSettingsQuery query, CancellationToken cancellationToken = default)
    {
        var settings = await notificationSettingsRepository.GetByUserIdOrCreateAsync(query.UserId, cancellationToken);
        
        var response = new NotificationSettingsDto(
            settings.UserId,
            settings.IsEnabled,
            settings.IsTelegramEnabled,
            settings.IsEmailEnabled,
            settings.IsPushEnabled,
            settings.NewTournamentPosts,
            settings.TournamentUpdates,
            settings.TournamentTeamInvites,
            settings.TeamChanged,
            settings.TeamInvites,
            settings.TournamentInvites,
            settings.AdminRoleAssigned,
            settings.TournamentLobbyInvites,
            settings.NewTournamentStep,
            settings.TournamentStarted,
            settings.NewFriendRequest,
            settings.NewMessage,
            settings.NewPostComment,
            settings.NewCommentReply,
            settings.NewCommentReaction
        );
        
        return ResultsHelper.Ok(response);
    }
}

public class GetNotificationSettingsQueryValidator : AbstractValidator<GetNotificationSettingsQuery>
{
    public GetNotificationSettingsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("ID пользователя обязателен");
    }
} 