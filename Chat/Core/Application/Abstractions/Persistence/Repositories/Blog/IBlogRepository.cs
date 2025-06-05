using Domain.Models;
using Domain.Models.Blog;

namespace Application.Abstractions.Persistence.Repositories.Blog;

public interface IBlogRepository : IBaseRepository<Post>
{
    public Task<ChatUser?> GetUserByIdWithPostsAsync(Guid requestUserId, CancellationToken cancellationToken = default);
}