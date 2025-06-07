using Domain.Models.Blog;
using Domain.Models.Users;

namespace Application.Abstractions.Persistence.Repositories.Blog;

public interface IBlogRepository : IBaseRepository<Post>
{
    public Task<ChatUser?> GetUserByIdWithPostsAsync(Guid requestUserId, CancellationToken cancellationToken = default);
}