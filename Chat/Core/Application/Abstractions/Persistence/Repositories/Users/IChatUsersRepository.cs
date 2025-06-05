using Application.Dtos.Requests.Shared;
using Domain.Models;

namespace Application.Abstractions.Persistence.Repositories.Users;

public interface IChatUsersRepository : IBaseRepository<ChatUser>
{
    public Task<List<ChatUser>> GetByUsernameAsync(string username, PageSettings requestPageSettings, CancellationToken cancellationToken = default);
    public Task<ChatUser?> GetByIdWithFriendsAsync(Guid requestUserId, CancellationToken cancellationToken);
    public Task<ChatUser?> GetByIdWithBlockerUsersAsync(Guid requestUserId, CancellationToken cancellationToken);
};