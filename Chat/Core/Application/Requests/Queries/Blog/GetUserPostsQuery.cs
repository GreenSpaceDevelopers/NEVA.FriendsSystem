using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Blog;

namespace Application.Requests.Queries.Blog;

public record GetUserPostsQuery(Guid UserId, PageSettings PageSettings, bool Desc = true) : IRequest;

public class GetUserPostsQueryHandler(IBlogRepository blogRepository) : IRequestHandler<GetUserPostsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserPostsQuery request, CancellationToken cancellationToken = default)
    {
        var user = await blogRepository.GetUserByIdWithPostsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        var posts = request.Desc
            ? user.Posts.OrderByDescending(p => p.CreatedAt)
            : user.Posts.OrderBy(p => p.CreatedAt);

        var paged = posts
            .Skip(request.PageSettings.Skip)
            .Take(request.PageSettings.Take)
            .Select(p => new PostListItemDto(
                p.Id,
                p.Title,
                p.Content,
                p.Attachment?.Url,
                p.CreatedAt,
                p.Comments?.Count ?? 0,
                p.Reactions?.Count ?? 0,
                p.IsPinned,
                p.IsCommentsEnabled
            ))
            .ToList();

        return ResultsHelper.Ok(paged);
    }
}