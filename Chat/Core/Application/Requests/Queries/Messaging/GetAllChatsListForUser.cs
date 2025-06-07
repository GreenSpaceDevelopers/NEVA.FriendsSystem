using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Dtos.Requests.Shared;

namespace Application.Requests.Queries.Messaging;

public record GetAllChatsForUserQuery(Guid UserId, PageSettings PageSettings) : IRequest;

public class GetAllChatsForUserQueryHandler(IChatUsersRepository chatUsersRepository, IChatsRepository chatsRepository) : IRequestHandler<GetAllChatsForUserQuery>
{
    public async Task<IOperationResult> HandleAsync(GetAllChatsForUserQuery request, CancellationToken cancellationToken = default)
    {
        var user = await chatUsersRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var chats = await chatsRepository.GetUserChatsNoTrackingAsync(user.Id, request.PageSettings, cancellationToken);

        var chatList = chats.Select(chat => chat.ToUserChatListItem(user.Id)).ToList();

        return ResultsHelper.Ok(chatList);
    }
}