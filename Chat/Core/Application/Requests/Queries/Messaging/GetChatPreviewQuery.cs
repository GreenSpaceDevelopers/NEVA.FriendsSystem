using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Messaging;

public record GetChatPreviewQuery(Guid UserId, Guid ChatId) : IRequest;

public class GetChatPreviewQueryHandler(IChatsRepository chatsRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetChatPreviewQuery>
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
            string? avatarUrl = null;
            if (!string.IsNullOrEmpty(user.Avatar?.Url))
            {
                avatarUrl = await filesSigningService.GetSignedUrlAsync(user.Avatar.Url, cancellationToken);
            }

            participants.Add(new ChatParticipantDto(user.Id, user.Username, user.PersonalLink, avatarUrl));
        }

        var isGroup = chat.Users.Count > 2;
        string displayName;
        Guid? friendId = null;
        
        if (!isGroup && chat.Users.Count == 2)
        {
            var interlocutor = chat.Users.First(u => u.Id != request.UserId);
            displayName = interlocutor.Username;
            friendId = interlocutor.Id;
        }
        else
        {
            displayName = chat.Name;
        }

        string? chatImageUrl = null;
        if (!string.IsNullOrEmpty(chat.ChatPicture?.Url))
        {
            chatImageUrl = await filesSigningService.GetSignedUrlAsync(chat.ChatPicture.Url, cancellationToken);
        }

        var lastMessage = chat.Messages.FirstOrDefault();
        var lastMsgPreview = new LastChatMessagePreview(lastMessage?.Sender.Username ?? string.Empty,
            lastMessage?.Content ?? string.Empty,
            lastMessage?.Attachments?.Count > 0,
            lastMessage?.CreatedAt ?? default);

        var dto = new ChatDetailsDto(chat.Id, displayName, chatImageUrl, isGroup, participants, lastMsgPreview, friendId);

        return ResultsHelper.Ok(dto);
    }
} 