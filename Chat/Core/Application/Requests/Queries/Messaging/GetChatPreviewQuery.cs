using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Messaging;

public record GetChatPreviewQuery(Guid UserId, Guid ChatId) : IRequest;

public class GetChatPreviewQueryHandler(IChatsRepository chatsRepository) : IRequestHandler<GetChatPreviewQuery>
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

        var participants = chat.Users.Select(u => new ChatParticipantDto(u.Id, u.Username, u.Avatar?.Url)).ToList();

        var isGroup = chat.Users.Count > 2;
        string displayName;
        if (!isGroup && chat.Users.Count == 2)
        {
            var interlocutor = chat.Users.First(u => u.Id != request.UserId);
            displayName = interlocutor.Username;
        }
        else
        {
            displayName = chat.Name;
        }

        var lastMessage = chat.Messages.FirstOrDefault();
        var lastMsgPreview = new LastChatMessagePreview(lastMessage?.Sender.Username ?? string.Empty,
            lastMessage?.Content ?? string.Empty,
            lastMessage?.Attachment is not null,
            lastMessage?.CreatedAt ?? default);

        var dto = new ChatDetailsDto(chat.Id, displayName, chat.ChatPicture?.Url, isGroup, participants, lastMsgPreview);

        return ResultsHelper.Ok(dto);
    }
} 