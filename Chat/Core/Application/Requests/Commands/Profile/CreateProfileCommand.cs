using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Service;
using Domain.Models.Users;

namespace Application.Requests.Commands.Profile;

public record CreateProfileCommand(AspNetUser AspNetUser) : IRequest;

public class CreateProfileCommandHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<CreateProfileCommand>
{
    public async Task<IOperationResult> HandleAsync(CreateProfileCommand request, CancellationToken cancellationToken = default)
    {
        var chatUser = new ChatUser(request.AspNetUser);

        var userPrivacySettings = new UserPrivacySettings
        {
            Id = Guid.NewGuid(),
            ChatUserId = chatUser.Id,
            FriendsListVisibility = PrivacyLevel.Public,
            CommentsPermission = PrivacyLevel.Public,
            DirectMessagesPermission = PrivacyLevel.Public,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        var notificationSetting = new NotificationSettings
        {
            Id = chatUser.Id,
            UserId = chatUser.Id,
            User = chatUser,
            IsEnabled = true,
            IsTelegramEnabled = true,
            IsEmailEnabled = true,
            IsPushEnabled = true,
            NewMessage = true,
            NewCommentReply = true,
            NewFriendRequest = true,
            NewCommentReaction = true,
            NewPostComment = true,
            NewTournamentStep = true,
            TournamentStarted = true,
            NewTournamentPosts = true,
            TournamentUpdates = true,
            TournamentTeamInvites = true,
            TeamChanged = true,
            TeamInvites = true,
            TournamentInvites = true,
            AdminRoleAssigned = true,
            TournamentLobbyInvites = true
        };

        chatUser.PrivacySettings = userPrivacySettings;

        await chatUsersRepository.AddAsync(chatUser, cancellationToken);
        await chatUsersRepository.SaveChangesAsync(cancellationToken);

        return ResultsHelper.NoContent();
    }
}