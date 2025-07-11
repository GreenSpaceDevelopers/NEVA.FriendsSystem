using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Abstractions.Persistence.Repositories.Users;
using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using Application.Abstractions.Services.ApplicationInfrastructure.Mediator;
using Application.Abstractions.Services.ApplicationInfrastructure.Results;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Application.Dtos.Responses.Blog;
using Application.Services.ApplicationInfrastructure.Results;

namespace Application.Requests.Queries.Blog;

public record GetUserPostsQuery(Guid UserId, PageSettings PageSettings, bool? Desc = true, Guid? CurrentUserId = null) : IRequest;

public class GetUserPostsQueryHandler(IBlogRepository blogRepository, IChatUsersRepository chatUsersRepository, IFilesSigningService filesSigningService) : IRequestHandler<GetUserPostsQuery>
{
    public async Task<IOperationResult> HandleAsync(GetUserPostsQuery request, CancellationToken cancellationToken = default)
    {
        var user = await blogRepository.GetUserByIdWithPostsAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return ResultsHelper.NotFound("User not found");
        }

        if (request.CurrentUserId.HasValue)
        {
            var isBlocked = await chatUsersRepository.IsUserBlockedByAsync(request.CurrentUserId.Value, request.UserId, cancellationToken);
            if (isBlocked)
            {
                return ResultsHelper.Forbidden("You are blocked by the author");
            }
        }

        var sortExpressions = new List<SortExpression>
        {
            new()
            {
                PropertyName = nameof(Domain.Models.Blog.Post.IsPinned),
                Direction = SortDirection.Desc
            },
            new()
            {
                PropertyName = nameof(Domain.Models.Blog.Post.CreatedAt),
                Direction = request.Desc == true ? SortDirection.Desc : SortDirection.Asc
            }
        };

        var pagedPosts = await blogRepository.GetUserPostsPagedAsync(
            request.UserId,
            request.PageSettings,
            sortExpressions,
            cancellationToken);

        var currentUserId = request.CurrentUserId;
        var postDtos = new List<PostListItemDto>();
        
        foreach (var post in pagedPosts.Data)
        {
            var attachmentUrl = post.Attachment?.Url != null 
                ? await filesSigningService.GetSignedUrlAsync(post.Attachment.Url, cancellationToken) 
                : null;
                
            var authorAvatarUrl = post.Author.Avatar?.Url != null 
                ? await filesSigningService.GetSignedUrlAsync(post.Author.Avatar.Url, cancellationToken) 
                : null;

            var postDto = new PostListItemDto(
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
            
            postDtos.Add(postDto);
        }

        var pagedResult = new PagedList<PostListItemDto>
        {
            TotalCount = pagedPosts.TotalCount,
            Data = postDtos
        };

        return ResultsHelper.Ok(pagedResult);
    }
}