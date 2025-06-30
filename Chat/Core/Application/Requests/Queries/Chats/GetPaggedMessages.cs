using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Chats;

namespace Application.Requests.Queries.Chats;

public record GetPaggedMessages(Guid ChatId, Guid UserId, int Skip, int Take) : IRequest;

public class GetPaggedMessagesHandler(IChatsRepository chatsRepository) : IRequestHandler<GetPaggedMessages>
{
    public async Task<IOperationResult> HandleAsync(GetPaggedMessages request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdAsync(request.ChatId, cancellationToken);
        
        if (chat is null)
        {
            return ResultsHelper.NotFound("Chat not found");
        }
        
        if (chat.Users.All(u => u.Id != request.UserId))
        {
            return ResultsHelper.Forbidden("You are not a member of this chat");
        }
        
        var messages = await chatsRepository.GetMessagesByChatIdNoTrackingAsync(request.ChatId, request.Take, request.Skip, cancellationToken);
        
        var messageDtos = messages.Select(m => new MessageDto(
            m.Id,
            m.ChatId,
            m.SenderId,
            m.Sender.Username,
            m.Sender.Avatar?.Url,
            m.Content,
            m.Attachment?.Url,
            m.CreatedAt,
            m.Replies.Count,
            m.Reactions.Count
        )).ToList();

        return ResultsHelper.Ok(new
        {
            messageDtos,
            chat.Messages.Count
        });
    }
}