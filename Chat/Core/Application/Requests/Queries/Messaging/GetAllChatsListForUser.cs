using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Chats;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Messaging;

public record GetAllChatsForUserQuery(Guid UserId, PageSettings PageSettings, string? SearchQuery = null) : IRequest;

public class GetAllChatsForUserQueryHandler(IChatUsersRepository chatUsersRepository, IChatsRepository chatsRepository) : IRequestHandler<GetAllChatsForUserQuery>
{
    public async Task<IOperationResult> HandleAsync(GetAllChatsForUserQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return ResultsHelper.NotFound("Пользователь не найден");
        }

        var chatsWithUnreadCount = await chatsRepository.GetUserChatsWithUnreadCountAsync(
            request.UserId, 
            request.PageSettings, 
            request.SearchQuery, 
            cancellationToken);

        var chatDtos = chatsWithUnreadCount.Data
            .Select(chatWithUnreadCount => chatWithUnreadCount.ToUserChatListItemDto(request.UserId))
            .ToList();

        var result = new PagedList<UserChatListItemDto>
        {
            Data = chatDtos,
            TotalCount = chatsWithUnreadCount.TotalCount
        };

        return ResultsHelper.Ok(result);
    }
}