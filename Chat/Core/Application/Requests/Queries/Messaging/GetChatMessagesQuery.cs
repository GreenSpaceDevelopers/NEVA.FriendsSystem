using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Messaging;

public record GetChatMessagesQuery(Guid ChatId, Guid UserId, PageSettings PageSettings, bool Desc = true) : IRequest;

public class GetChatMessagesQueryHandler(
    IChatsRepository chatsRepository,
    IUserChatSettingsRepository userChatSettingsRepository) : IRequestHandler<GetChatMessagesQuery>
{
    public async Task<IOperationResult> HandleAsync(GetChatMessagesQuery request, CancellationToken cancellationToken = default)
    {
        var chat = await chatsRepository.GetByIdWithUsersAsync(request.ChatId, cancellationToken);

        if (chat is null)
        {
            return ResultsHelper.NotFound("Чат не найден");
        }

        if (chat.Users.All(u => u.Id != request.UserId))
        {
            return ResultsHelper.Forbidden("Вы не являетесь участником данного чата");
        }

        var messages = await chatsRepository.GetMessagesByChatIdNoTrackingAsync(
            request.ChatId, 
            request.PageSettings.Take, 
            request.PageSettings.Skip, 
            cancellationToken);

        if (messages.Count != 0)
        {
            var lastMessage = messages.OrderByDescending(m => m.CreatedAt).First();
            
            var userChatSettings = await userChatSettingsRepository.GetByUserAndChatOrCreateAsync(
                request.UserId, 
                request.ChatId, 
                cancellationToken);

            if (userChatSettings.LastReadMessageId == null || 
                messages.Any(m => m.Id == userChatSettings.LastReadMessageId && m.CreatedAt < lastMessage.CreatedAt))
            {
                userChatSettings.LastReadMessageId = lastMessage.Id;
                userChatSettingsRepository.UpdateAsync(userChatSettings, cancellationToken);
                await userChatSettingsRepository.SaveChangesAsync(cancellationToken);
            }
        }

        var messagesDtos = messages.Select(message => message.ToMessageDto()).ToList();

        var pagedResult = new PagedList<MessageDto>
        {
            Data = messagesDtos,
            TotalCount = messages.Count // TODO: получать правильное общее количество через отдельный запрос
        };

        return ResultsHelper.Ok(pagedResult);
    }
} 