using Application.Abstractions.Persistence.Repositories.Messaging;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Mappers;
using Application.Dtos.Requests.Shared;

namespace Application.Requests.Queries.BlackList;

public record GetUserBlackListQuery(Guid UserId, string Query, PageSettings PageSettings) : IRequest;

public class GetUserBlackListRequest(IChatUsersRepository chatUsersRepository, IChatsRepository chatsRepository)
    : IRequestHandler<GetUserBlackListQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserBlackListQuery request,
        CancellationToken cancellationToken = default)
    {
        var blockedUsers = await chatUsersRepository.GetBlockedUsersAsync(request.UserId, request.Query, request.PageSettings, cancellationToken);

        var userBlockList = blockedUsers.Select(bu => bu.ToBlackListItem()).ToList();
        
        return ResultsHelper.Ok(userBlockList);
    }
}