using Application.Abstractions.Persistence.Repositories.Blog;
using Domain.Models.Blog;
using Domain.Models.Users;
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
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
    }
} 