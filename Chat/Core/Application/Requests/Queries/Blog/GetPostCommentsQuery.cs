using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Blog;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Blog;

public record GetPostCommentsQuery(Guid PostId, PageSettings PageSettings, Guid? CurrentUserId = null) : IRequest;

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

        var sortExpressions = new List<SortExpression>
        {
            new()
            {
                PropertyName = nameof(Domain.Models.Blog.Comment.CreatedAt),
                Direction = SortDirection.Desc
            }
        };

        var pagedComments = await blogRepository.GetPostCommentsPagedAsync(
            request.PostId,
            request.PageSettings,
            sortExpressions,
            cancellationToken);

        var currentUserId = request.CurrentUserId;
        var pagedResult = pagedComments.Map(c => new CommentDto(
            c.Id,
            c.Content,
            c.Attachment?.Url,
            c.CreatedAt,
            c.Author.Id,
            c.Author.Username,
            c.Author.Avatar?.Url,
            c.ParentCommentId,
            pagedComments.Data.Count(reply => reply.ParentCommentId == c.Id),
            c.CommentReactions?.Count ?? 0,
            currentUserId.HasValue && c.CommentReactions != null && c.CommentReactions.Any(r => r.UserId == currentUserId.Value)
        ));

        return ResultsHelper.Ok(pagedResult);
    }
}