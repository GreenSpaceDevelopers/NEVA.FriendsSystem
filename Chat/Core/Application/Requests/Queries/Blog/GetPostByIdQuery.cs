using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Blog;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Blog;

public record GetPostByIdQuery(Guid PostId, Guid? CurrentUserId = null) : IRequest;

public class GetPostByIdQueryHandler(IBlogRepository blogRepository) : IRequestHandler<GetPostByIdQuery>
{
    public async Task<IOperationResult> HandleAsync(GetPostByIdQuery request, CancellationToken cancellationToken = default)
    {
        var post = await blogRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        var currentUserId = request.CurrentUserId;
        var dto = new PostListItemDto(
            post.Id,
            post.Title ?? string.Empty,
            post.Content,
            post.Attachment?.Url,
            post.CreatedAt,
            post.Comments?.Count ?? 0,
            post.Reactions?.Count ?? 0,
            post.IsPinned,
            post.IsCommentsEnabled,
            post.Author.Id,
            post.Author.Username,
            post.Author.Avatar?.Url,
            currentUserId.HasValue && (post.Reactions?.Any(r => r.UserId == currentUserId.Value) ?? false)
        );

        return ResultsHelper.Ok(dto);
    }
} 