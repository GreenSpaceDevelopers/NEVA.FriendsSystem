using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Dtos.Responses.Blog;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Blog;

public record GetPostByIdQuery(Guid PostId, Guid? CurrentUserId = null) : IRequest;

public class GetPostByIdQueryHandler(IBlogRepository blogRepository, IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetPostByIdQuery>
{
    public async Task<IOperationResult> HandleAsync(GetPostByIdQuery request, CancellationToken cancellationToken = default)
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

        var attachmentUrl = post.Attachment?.Url != null 
            ? await filesSigningService.GetSignedUrlAsync(post.Attachment.Url, cancellationToken) 
            : null;
            
        var authorAvatarUrl = post.Author.Avatar?.Url != null 
            ? await filesSigningService.GetSignedUrlForObjectAsync(post.Author.Avatar.Url, post.Author.Avatar.BucketName ?? "neva-avatars", cancellationToken) 
            : null;

        var currentUserId = request.CurrentUserId;
        var dto = new PostListItemDto(
            post.Id,
            post.Title ?? string.Empty,
            post.Content,
            attachmentUrl,
            post.CreatedAt,
            post.Comments?.Count ?? 0,
            post.Reactions?.Count ?? 0,
            post.IsPinned,
            post.IsCommentsEnabled,
            post.Author.Id,
            post.Author.Username,
            post.Author.PersonalLink,
            authorAvatarUrl,
            currentUserId.HasValue && (post.Reactions?.Any(r => r.ReactorId == currentUserId.Value) ?? false)
        );

        return ResultsHelper.Ok(dto);
    }
} 