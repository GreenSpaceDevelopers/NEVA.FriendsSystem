using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Profile;
using Application.Dtos.Responses.Profile;
using Domain.Models.Service;
using FluentValidation;

namespace Application.Requests.Commands.Profile;

public record UpdateNotificationSettingsCommand(UpdateNotificationSettingsRequest Request) : IRequest;

public class UpdateNotificationSettingsCommandHandler(INotificationSettingsRepository notificationSettingsRepository) 
    : IRequestHandler<UpdateNotificationSettingsCommand>
{
    public async Task<IOperationResult> HandleAsync(UpdateNotificationSettingsCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        
        var settings = await notificationSettingsRepository.GetByUserIdOrCreateAsync(request.UserId, cancellationToken);
        
        if (request.IsEnabled.HasValue)
            settings.IsEnabled = request.IsEnabled.Value;
            
        if (request.IsTelegramEnabled.HasValue)
            settings.IsTelegramEnabled = request.IsTelegramEnabled.Value;
            
        if (request.IsEmailEnabled.HasValue)
            settings.IsEmailEnabled = request.IsEmailEnabled.Value;
            
        if (request.IsPushEnabled.HasValue)
            settings.IsPushEnabled = request.IsPushEnabled.Value;
            
        if (request.NewTournamentPosts.HasValue)
            settings.NewTournamentPosts = request.NewTournamentPosts.Value;
            
        if (request.TournamentUpdates.HasValue)
            settings.TournamentUpdates = request.TournamentUpdates.Value;
            
        if (request.TournamentTeamInvites.HasValue)
            settings.TournamentTeamInvites = request.TournamentTeamInvites.Value;
            
        if (request.TeamChanged.HasValue)
            settings.TeamChanged = request.TeamChanged.Value;
            
        if (request.TeamInvites.HasValue)
            settings.TeamInvites = request.TeamInvites.Value;
            
        if (request.TournamentInvites.HasValue)
            settings.TournamentInvites = request.TournamentInvites.Value;
            
        if (request.AdminRoleAssigned.HasValue)
            settings.AdminRoleAssigned = request.AdminRoleAssigned.Value;
            
        if (request.TournamentLobbyInvites.HasValue)
            settings.TournamentLobbyInvites = request.TournamentLobbyInvites.Value;
            
        if (request.NewTournamentStep.HasValue)
            settings.NewTournamentStep = request.NewTournamentStep.Value;
            
        if (request.TournamentStarted.HasValue)
            settings.TournamentStarted = request.TournamentStarted.Value;
            
        if (request.NewFriendRequest.HasValue)
            settings.NewFriendRequest = request.NewFriendRequest.Value;
            
        if (request.NewMessage.HasValue)
            settings.NewMessage = request.NewMessage.Value;
            
        if (request.NewPostComment.HasValue)
            settings.NewPostComment = request.NewPostComment.Value;
            
        if (request.NewCommentReply.HasValue)
            settings.NewCommentReply = request.NewCommentReply.Value;
            
        if (request.NewCommentReaction.HasValue)
            settings.NewCommentReaction = request.NewCommentReaction.Value;
        
        notificationSettingsRepository.Update(settings);
        await notificationSettingsRepository.SaveChangesAsync(cancellationToken);
        
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

public class UpdateNotificationSettingsCommandValidator : AbstractValidator<UpdateNotificationSettingsCommand>
{
    public UpdateNotificationSettingsCommandValidator()
    {
        RuleFor(x => x.Request.UserId)
            .NotEmpty()
            .WithMessage("ID пользователя обязателен");
    }
} 