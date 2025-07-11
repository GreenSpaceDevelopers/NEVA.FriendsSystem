using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.BlackList;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.BlackList;

public record SearchUsersQuery(Guid CurrentUserId, string? Query, PageSettings PageSettings) : IRequest;

public class SearchUsersQueryHandler(IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService) : IRequestHandler<SearchUsersQuery>
{
    public async Task<IOperationResult> HandleAsync(SearchUsersQuery request, CancellationToken cancellationToken = default)
    {
        var usersWithBlockingInfo = await chatUsersRepository.SearchUsersWithBlockingInfoAsync(
            request.Query,
            request.PageSettings,
            request.CurrentUserId,
            cancellationToken);

        var userDtos = new List<UserSearchDto>();
        foreach (var userInfo in usersWithBlockingInfo.Data)
        {
            string? avatarUrl = null;
            if (!string.IsNullOrEmpty(userInfo.User.Avatar?.Url))
            {
                avatarUrl = await filesSigningService.GetSignedUrlAsync(userInfo.User.Avatar.Url, cancellationToken);
            }

            var userDto = new UserSearchDto(
                userInfo.User.Id,
                userInfo.User.Username,
                userInfo.User.PersonalLink,
                avatarUrl,
                userInfo.IsBlockedByMe,
                userInfo.HasBlockedMe,
                userInfo.IsFriendRequestSentByMe
            );
            userDtos.Add(userDto);
        }

        var pagedResult = new PagedList<UserSearchDto>
        {
            Data = userDtos,
            TotalCount = usersWithBlockingInfo.TotalCount
        };

        return ResultsHelper.Ok(pagedResult);
    }
}