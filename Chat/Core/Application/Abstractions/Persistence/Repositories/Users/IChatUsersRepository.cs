using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Users;

namespace Application.Abstractions.Persistence.Repositories.Users;

public interface IChatUsersRepository : IBaseRepository<ChatUser>
{
    public Task<PagedList<ChatUser>> GetByUsernamePagedAsync(string? username, PageSettings requestPageSettings, Guid currentUserId, IEnumerable<Guid> blockedUserIds, CancellationToken cancellationToken = default);
    public Task<ChatUser?> GetByIdWithFriendsAsync(Guid requestUserId, CancellationToken cancellationToken);
    public Task<ChatUser?> GetByIdWithBlockerUsersAsync(Guid requestUserId, CancellationToken cancellationToken);
    public Task<List<ChatUser>> GetBlockedUsersAsync(Guid requestUserId, string queryString, PageSettings requestPageSettings, CancellationToken cancellationToken);
    public Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default);
    public Task<ChatUser?> GetByIdWithProfileDataAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<PagedList<ChatUser>> GetBlockedUsersPagedAsync(Guid requestUserId, string? queryString, PageSettings requestPageSettings, CancellationToken cancellationToken = default);
}