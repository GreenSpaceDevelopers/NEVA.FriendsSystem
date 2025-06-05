using Application.Abstractions.Persistence.Repositories.Users;
using Application.Dtos.Requests.Shared;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Users;

public class ChatChatUsersRepository(ChatsDbContext dbContext) : BaseRepository<ChatUser>(dbContext), IChatUsersRepository
{
    public Task<List<ChatUser>> GetByUsernameAsync(string username, PageSettings requestPageSettings, CancellationToken cancellationToken = default)
    {
        return _dbContext.Set<ChatUser>()
            .Where(user => user.Username.StartsWith(username))
            .Skip(requestPageSettings.PageSize * (requestPageSettings.PageNumber - 1))
            .Take(requestPageSettings.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public Task<ChatUser?> GetByIdWithFriendsAsync(Guid requestUserId, CancellationToken cancellationToken)
    {
        return _dbContext.Set<ChatUser>()
            .Include(user => user.Friends)
            .Include(user => user.FriendRequests)
            .Include(user => user.BlockedUsers)
            .Include(user => user.WaitingFriendRequests)
            .SingleOrDefaultAsync(user => user.Id == requestUserId, cancellationToken: cancellationToken);
    }

    public Task<ChatUser?> GetByIdWithBlockerUsersAsync(Guid requestUserId, CancellationToken cancellationToken)
    {
        return _dbContext.Set<ChatUser>()
            .Include(user => user.BlockedUsers)
            .SingleOrDefaultAsync(user => user.Id == requestUserId, cancellationToken: cancellationToken);
    }
}