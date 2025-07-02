using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Blog;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Blog;

public record GetUserPostsQuery(Guid UserId, PageSettings PageSettings, bool? Desc = true, Guid? CurrentUserId = null) : IRequest;

public class GetUserPostsQueryHandler(IBlogRepository blogRepository) : IRequestHandler<GetUserPostsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserPostsQuery request, CancellationToken cancellationToken = default)
    {
        var user = await blogRepository.GetUserByIdWithPostsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var sortExpressions = new List<Common.Models.SortExpression>
        {
            new()
            {
                PropertyName = nameof(Domain.Models.Blog.Post.CreatedAt),
                Direction = request.Desc == true ? Common.Models.SortDirection.Desc : Common.Models.SortDirection.Asc
            }
        };

        var pagedPosts = await blogRepository.GetUserPostsPagedAsync(
            request.UserId,
            request.PageSettings,
            sortExpressions,
            cancellationToken);

        var currentUserId = request.CurrentUserId;
        var pagedResult = pagedPosts.Map(p => new PostListItemDto(
            p.Id,
            p.Title ?? string.Empty,
            p.Content,
            p.Attachment?.Url,
            p.CreatedAt,
            p.Comments?.Count ?? 0,
            p.Reactions?.Count ?? 0,
            p.IsPinned,
            p.IsCommentsEnabled,
            p.Author.Id,
            p.Author.Username,
            p.Author.Avatar?.Url,
            currentUserId.HasValue && (p.Reactions?.Any(r => r.UserId == currentUserId.Value) ?? false)
        ));

        return ResultsHelper.Ok(pagedResult);
    }
}