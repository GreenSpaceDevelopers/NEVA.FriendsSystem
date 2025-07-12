using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Blog;
using Application.Dtos.Responses.Shared;
using Application.Services.ApplicationInfrastructure.Results;
using Domain.Models.Blog;

namespace Application.Requests.Queries.Blog;

public record GetPostCommentsQuery(Guid PostId, PageSettings PageSettings, Guid? CurrentUserId = null) : IRequest;

public class GetPostCommentsQueryHandler(IBlogRepository blogRepository, IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetPostCommentsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetPostCommentsQuery request, CancellationToken cancellationToken = default)
    {
        var post = await blogRepository.GetPostByIdWithDetailsAsync(request.PostId, cancellationToken);
        if (post is null)
        {
            return ResultsHelper.NotFound("Post not found");
        }

        if (request.CurrentUserId.HasValue)
        {
            var isBlocked = await chatUsersRepository.IsUserBlockedByAsync(request.CurrentUserId.Value, post.Author.Id, cancellationToken);
            if (isBlocked)
            {
                return ResultsHelper.Forbidden("You are blocked by the author");
            }
        }

        var sortExpressions = new List<SortExpression>
        {
            new()
            {
                PropertyName = nameof(Comment.CreatedAt),
                Direction = SortDirection.Asc
            }
        };

        var comments = await blogRepository.GetPostCommentsPagedAsync(
            request.PostId,
            request.PageSettings,
            sortExpressions,
            cancellationToken);

        var commentDtos = new List<CommentDto>();
        
        foreach (var comment in comments.Data)
        {
            var commentDto = await ConvertToCommentDto(comment, request.CurrentUserId, filesSigningService, cancellationToken);
            commentDtos.Add(commentDto);
        }

        var pagedResult = new PagedList<CommentDto>
        {
            TotalCount = comments.TotalCount,
            Data = commentDtos
        };

        return ResultsHelper.Ok(pagedResult);
    }

    private static async Task<CommentDto> ConvertToCommentDto(Comment comment, Guid? currentUserId, IFilesSigningService filesSigningService, CancellationToken cancellationToken)
    {
        var attachments = new List<AttachmentDto>();
        if (comment.Attachments.Count != 0)
        {
            foreach (var attachment in comment.Attachments.Where(attachment => !string.IsNullOrEmpty(attachment.Url)))
            {
                var signedUrl = await filesSigningService.GetSignedUrlAsync(attachment.Url, cancellationToken);
                attachments.Add(new AttachmentDto(attachment.Id, signedUrl));
            }
        }
            
        var authorAvatarUrl = comment.Author.Avatar?.Url != null 
            ? await filesSigningService.GetSignedUrlAsync(comment.Author.Avatar.Url, cancellationToken) 
            : null;

        var replies = new List<CommentDto>();
        if (comment.Replies?.Count > 0)
        {
            foreach (var reply in comment.Replies)
            {
                var replyDto = await ConvertToCommentDto(reply, currentUserId, filesSigningService, cancellationToken);
                replies.Add(replyDto);
            }
        }

        return new CommentDto(
            comment.Id,
            comment.Content,
            attachments,
            comment.CreatedAt,
            comment.AuthorId,
            comment.Author.Username,
            comment.Author.PersonalLink,
            authorAvatarUrl,
            comment.ParentCommentId,
            comment.Replies?.Count ?? 0,
            comment.CommentReactions?.Count ?? 0,
            currentUserId.HasValue && (comment.CommentReactions?.Any(r => r.ReactorId == currentUserId.Value) ?? false),
            replies
        );
    }
}