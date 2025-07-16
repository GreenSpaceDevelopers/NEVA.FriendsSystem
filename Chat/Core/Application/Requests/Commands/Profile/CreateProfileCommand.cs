using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Persistence.Repositories.Media;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Service;
using Domain.Models.Users;
using Domain.Models.Messaging;

namespace Application.Requests.Commands.Profile;

public record CreateProfileCommand(AspNetUser AspNetUser, string? ImageLink = null) : IRequest;

public class CreateProfileCommandHandler(
    IChatUsersRepository chatUsersRepository,
    IAttachmentsRepository attachmentsRepository) : IRequestHandler<CreateProfileCommand>
{
    public async Task<IOperationResult> HandleAsync(CreateProfileCommand request, CancellationToken cancellationToken = default)
    {
        var chatUser = new ChatUser(request.AspNetUser);
        chatUser.PersonalLink = chatUser.Username;

        if (!string.IsNullOrEmpty(request.ImageLink))
        {
            var avatarType = await attachmentsRepository.GetAttachmentTypeAsync(AttachmentTypes.Image, cancellationToken);
            
            var avatarAttachment = new Attachment
            {
                Id = Guid.NewGuid(),
                Url = request.ImageLink,
                Type = avatarType,
                TypeId = avatarType.Id,
            };

            await attachmentsRepository.AddAsync(avatarAttachment, cancellationToken);
            chatUser.Avatar = avatarAttachment;
            chatUser.AvatarId = avatarAttachment.Id;
        }

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