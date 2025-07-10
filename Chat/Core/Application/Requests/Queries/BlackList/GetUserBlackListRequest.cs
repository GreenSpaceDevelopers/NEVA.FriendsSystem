using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Common.Mappers;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.BlackList;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.BlackList;

public record GetUserBlackListQuery(Guid UserId, string? Query, PageSettings PageSettings) : IRequest;

public class GetUserBlackListRequest(IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService)
    : IRequestHandler<GetUserBlackListQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserBlackListQuery request,
        CancellationToken cancellationToken = default)
    {
        var blockedUsers = await chatUsersRepository.GetBlockedUsersPagedAsync(
            request.UserId,
            request.Query,
            request.PageSettings,
            cancellationToken);

        var blackListItems = new List<BlackListItemDto>();
        foreach (var user in blockedUsers.Data)
        {
            var blackListItem = await user.ToBlackListItemAsync(filesSigningService, cancellationToken);
            blackListItems.Add(blackListItem);
        }

        var pagedResult = new Application.Common.Models.PagedList<BlackListItemDto>
        {
            Data = blackListItems,
            TotalCount = blockedUsers.TotalCount
        };
        
        return ResultsHelper.Ok(pagedResult);
    }
}