using Application.Abstractions.Persistence.Repositories.Blog;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Blog;
using Domain.Models.Messaging;
using Domain.Models.Users;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Blog;

public class BlogRepository(ChatsDbContext dbContext) : BaseRepository<Post>(dbContext), IBlogRepository
{
    public async Task<ChatUser?> GetUserByIdWithPostsAsync(Guid requestUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<ChatUser>()
            .Include(u => u.Posts)
            .ThenInclude(p => p.Reactions)
            .Include(u => u.Posts)
            .ThenInclude(p => p.Comments)
            .FirstOrDefaultAsync(u => u.Id == requestUserId, cancellationToken);
    }

    public async Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<Comment>()
            .Include(c => c.CommentReactions)
            .Include(c => c.Author)
            .Include(c => c.Post)
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
    }

    public Task<PagedList<Post>> GetUserPostsPagedAsync(
        Guid userId,
        PageSettings pageSettings,
        List<SortExpression>? sortExpressions,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Post>()
            // .AsNoTracking() // TODO: надо исправить херобору с цикличностью
            .Include(p => p.Reactions)
            .Include(p => p.Comments)
            .Where(p => p.AuthorId == userId);

        return query.ToPagedList(sortExpressions, pageSettings.Skip, pageSettings.Take, cancellationToken);
    }

    public Task<PagedList<Comment>> GetPostCommentsPagedAsync(
        Guid postId,
        PageSettings pageSettings,
        List<SortExpression>? sortExpressions,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<Comment>()
            .AsNoTracking()
            .Include(c => c.CommentReactions)
            .Include(c => c.Author)
            .Include(c => c.Replies)
            .Where(c => c.PostId == postId && !c.ParentCommentId.HasValue);

        return query.ToPagedList(sortExpressions, pageSettings.Skip, pageSettings.Take, cancellationToken);
    }

    public async Task AddCommentAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<Comment>().AddAsync(comment, cancellationToken);
    }

    public async Task AddPostReactionAsync(PostReaction reaction, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<PostReaction>().AddAsync(reaction, cancellationToken);
    }

    public Task RemovePostReactionAsync(PostReaction reaction, CancellationToken cancellationToken = default)
    {
        dbContext.Set<PostReaction>().Remove(reaction);
        return Task.CompletedTask;
    }

    public async Task AddCommentReactionAsync(CommentReaction reaction, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<CommentReaction>().AddAsync(reaction, cancellationToken);
    }

    public Task RemoveCommentReactionAsync(CommentReaction reaction, CancellationToken cancellationToken = default)
    {
        dbContext.Set<CommentReaction>().Remove(reaction);
        return Task.CompletedTask;
    }
}