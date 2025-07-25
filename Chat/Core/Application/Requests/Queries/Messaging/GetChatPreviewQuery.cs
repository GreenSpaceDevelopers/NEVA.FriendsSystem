using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Messaging;

public record GetChatPreviewQuery(Guid UserId, Guid ChatId) : IRequest;

public class GetChatPreviewQueryHandler(IChatsRepository chatsRepository, IUserChatSettingsRepository userChatSettingsRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetChatPreviewQuery>
{
    public async Task<IOperationResult> HandleAsync(GetChatPreviewQuery request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetChatPreviewAsync(request.ChatId, cancellationToken);
        if (chat is null)
        {
            return ResultsHelper.NotFound("Chat not found");
        }

        if (chat.Users.All(u => u.Id != request.UserId))
        {
            return ResultsHelper.Forbidden("You are not a member of this chat");
        }

        var participants = new List<ChatParticipantDto>();
        foreach (var user in chat.Users)
        {
            string? avatarUrl;
            if (!string.IsNullOrEmpty(user.Avatar?.Url))
            {
                avatarUrl = await filesSigningService.GetSignedUrlForObjectAsync(user.Avatar.Url, user.Avatar.BucketName ?? "neva-avatars", cancellationToken);
            }
            else
            {
                avatarUrl = "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png";
            }

            participants.Add(new ChatParticipantDto(user.Id, user.Username, user.PersonalLink, avatarUrl));
        }

        string displayName;
        Guid? friendId = null;
        
        string? chatImageUrl = null;
        if (chat is { IsGroup: false })
        {
            var interlocutor = chat.Users.First(u => u.Id != request.UserId);
            displayName = interlocutor.Username;
            friendId = interlocutor.Id;
            if (!string.IsNullOrEmpty(interlocutor.Avatar?.Url))
            {
                chatImageUrl = await filesSigningService.GetSignedUrlForObjectAsync(interlocutor.Avatar.Url, "neva-avatars", cancellationToken);
            }
            else
            {
                chatImageUrl = "https://minio.greenspacegg.ru:9000/testpics/UserAvatar1.png";
            }
        }
        else
        {
            displayName = chat.Name;
            if (!string.IsNullOrEmpty(chat.ChatPicture?.Url))
            {
                chatImageUrl = await filesSigningService.GetSignedUrlAsync(chat.ChatPicture.Url, cancellationToken);
            }
        }
        

        var lastMessage = chat.Messages.FirstOrDefault();
        var lastMsgPreview = new LastChatMessagePreview(lastMessage?.Sender.Username ?? string.Empty,
            lastMessage?.Content ?? string.Empty,
            lastMessage?.Attachment is not null,
            lastMessage?.CreatedAt ?? default);

        var userSettings = await userChatSettingsRepository.GetByUserAndChatOrCreateAsync(request.UserId, chat.Id, cancellationToken);
        var isMuted = userSettings.IsMuted;

        if (lastMessage != null && lastMessage.SenderId != request.UserId)
        {
            if (userSettings.LastReadMessageId == null || userSettings.LastReadMessageId != lastMessage.Id)
            {
                userSettings.LastReadMessageId = lastMessage.Id;
                userChatSettingsRepository.UpdateAsync(userSettings, cancellationToken);
                await userChatSettingsRepository.SaveChangesAsync(cancellationToken);
            }
        }

        var dto = new ChatDetailsDto(chat.Id, displayName, chatImageUrl, chat.IsGroup, participants, lastMsgPreview, friendId, chat.AdminId, isMuted);

        return ResultsHelper.Ok(dto);
    }
} 