using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Chats;

public record GetPaggedMessages(Guid ChatId, Guid UserId, int Skip, int Take) : IRequest;

public class GetPaggedMessagesHandler(IChatsRepository chatsRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetPaggedMessages>
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
        
        var messageDtos = new List<MessageDto>();
        foreach (var message in messages)
        {
            var messageDto = await message.ToMessageDtoAsync(filesSigningService, cancellationToken);
            messageDtos.Add(messageDto);
        }

        return ResultsHelper.Ok(new
        {
            messageDtos,
            chat.Messages.Count
        });
    }
}