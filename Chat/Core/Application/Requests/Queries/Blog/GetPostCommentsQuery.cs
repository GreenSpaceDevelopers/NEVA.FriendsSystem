using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Blog;

namespace Application.Requests.Queries.Blog;

public record GetPostCommentsQuery(Guid PostId, PageSettings PageSettings) : IRequest;

public class GetPostCommentsQueryHandler(IBlogRepository blogRepository) : IRequestHandler<GetPostCommentsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetPostCommentsQuery request, CancellationToken cancellationToken = default)
    {
        var post = await blogRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        if (!post.IsCommentsEnabled)
        {
            return ResultsHelper.BadRequest("Comments are disabled for this post");
        }

        var comments = post.Comments
            .Where(c => !c.ParentCommentId.HasValue) // Только корневые комментарии
            .OrderByDescending(c => c.CreatedAt)
            .Skip(request.PageSettings.Skip)
            .Take(request.PageSettings.Take)
            .Select(c => new CommentDto(
                c.Id,
                c.Content,
                c.Attachment?.Url,
                c.CreatedAt,
                c.Author.Id,
                c.Author.Username,
                c.Author.Avatar?.Url,
                c.ParentCommentId,
                post.Comments.Count(reply => reply.ParentCommentId == c.Id),
                c.CommentReactions?.Count ?? 0
            ))
            .ToList();

        return ResultsHelper.Ok(comments);
    }
}