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
        var pagedResult = pagedComments.Map(c => MapCommentToDto(c, pagedComments.Data, currentUserId));

        return ResultsHelper.Ok(pagedResult);
    }

    private static CommentDto MapCommentToDto(Domain.Models.Blog.Comment comment, IReadOnlyCollection<Domain.Models.Blog.Comment> allComments, Guid? currentUserId)
    {
        return new CommentDto(
            comment.Id,
            comment.Content,
            comment.Attachment?.Url,
            comment.CreatedAt,
            comment.Author.Id,
            comment.Author.Username,
            comment.Author.Avatar?.Url,
            comment.ParentCommentId,
            comment.Replies?.Count ?? 0,
            comment.CommentReactions?.Count ?? 0,
            currentUserId.HasValue && comment.CommentReactions != null && comment.CommentReactions.Any(r => r.UserId == currentUserId.Value),
            (comment.Replies ?? []).Select(r => MapCommentToDto(r, allComments, currentUserId)).ToList()
        );
    }
}