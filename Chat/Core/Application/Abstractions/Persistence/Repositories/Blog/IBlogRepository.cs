using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Blog;
using Domain.Models.Messaging;
using Domain.Models.Users;

namespace Application.Abstractions.Persistence.Repositories.Blog;

public interface IBlogRepository : IBaseRepository<Post>
{
    public Task<ChatUser?> GetUserByIdWithPostsAsync(Guid requestUserId, CancellationToken cancellationToken = default);
    public Task<ChatUser?> GetUserByPersonalLinkWithPostsAsync(string personalLink, CancellationToken cancellationToken = default);
    public Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default);
    public Task<PagedList<Post>> GetUserPostsPagedAsync(
        Guid userId,
        PageSettings pageSettings,
        List<SortExpression>? sortExpressions,
        CancellationToken cancellationToken = default);
    public Task<PagedList<Comment>> GetPostCommentsPagedAsync(
        Guid postId,
        PageSettings pageSettings,
        List<SortExpression>? sortExpressions,
        CancellationToken cancellationToken = default);
    public Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default);
    public Task AddPostReactionAsync(PostReaction reaction, CancellationToken cancellationToken = default);
    public Task RemovePostReactionAsync(PostReaction reaction, CancellationToken cancellationToken = default);
    public Task AddCommentReactionAsync(CommentReaction reaction, CancellationToken cancellationToken = default);
    public Task RemoveCommentReactionAsync(CommentReaction reaction, CancellationToken cancellationToken = default);
    public Task<Post?> GetPostByIdWithDetailsAsync(Guid postId, CancellationToken cancellationToken = default);
}