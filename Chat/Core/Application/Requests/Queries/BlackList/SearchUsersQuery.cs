using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.BlackList;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.BlackList;

public record SearchUsersQuery(Guid CurrentUserId, string? Query, PageSettings PageSettings) : IRequest;

public class SearchUsersQueryHandler(IChatUsersRepository chatUsersRepository) : IRequestHandler<SearchUsersQuery>
{
    public async Task<IOperationResult> HandleAsync(SearchUsersQuery request, CancellationToken cancellationToken = default)
    {
        var currentUser = await chatUsersRepository.GetByIdWithBlockerUsersAsync(request.CurrentUserId, cancellationToken);
        if (currentUser is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var users = await chatUsersRepository.GetByUsernamePagedAsync(
            request.Query,
            request.PageSettings,
            request.CurrentUserId,
            currentUser.BlockedUsers.Select(b => b.Id),
            cancellationToken);

        var pagedResult = users.Map(u => new UserSearchDto(
            u.Id,
            u.Username,
            u.Avatar?.Url
        ));

        return ResultsHelper.Ok(pagedResult);
    }
}