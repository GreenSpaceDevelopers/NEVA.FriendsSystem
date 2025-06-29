using Application.Abstractions.Persistence.Repositories.Users;
using Application.Dtos.Requests.Shared;
using Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Users;

public class ChatChatUsersRepository(ChatsDbContext dbContext) : BaseRepository<ChatUser>(dbContext), IChatUsersRepository
{
    public Task<List<ChatUser>> GetByUsernameAsync(string username, PageSettings requestPageSettings, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .Where(user => user.Username.StartsWith(username))
            .Skip(requestPageSettings.PageSize * (requestPageSettings.PageNumber - 1))
            .Take(requestPageSettings.PageSize)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public Task<ChatUser?> GetByIdWithFriendsAsync(Guid requestUserId, CancellationToken cancellationToken)
    {
        return dbContext.Set<ChatUser>()
            .Include(user => user.Friends)
            .Include(user => user.FriendRequests)
            .Include(user => user.BlockedUsers)
            .Include(user => user.WaitingFriendRequests)
            .SingleOrDefaultAsync(user => user.Id == requestUserId, cancellationToken: cancellationToken);
    }

    public Task<ChatUser?> GetByIdWithBlockerUsersAsync(Guid requestUserId, CancellationToken cancellationToken)
    {
        return dbContext.Set<ChatUser>()
            .Include(user => user.BlockedUsers)
            .SingleOrDefaultAsync(user => user.Id == requestUserId, cancellationToken: cancellationToken);
    }

    public Task<List<ChatUser>> GetBlockedUsersAsync(
        Guid requestUserId,
        string queryString,
        PageSettings requestPageSettings,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Set<ChatUser>()
            .Where(u => u.BlockedUsers.Any(b => b.Id == requestUserId))
            .Include(u => u.AspNetUser)
            .Include(u => u.Avatar)
            .OrderBy(u => u.Username)
            .Skip((requestPageSettings.PageNumber - 1) * requestPageSettings.PageSize)
            .Take(requestPageSettings.PageSize);

        if (!string.IsNullOrEmpty(queryString))
        {
            query = query.Where(u => u.Username.Contains(queryString));
        }

        return query.ToListAsync(cancellationToken);
    }

    public Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .AnyAsync(user => user.Username == username, cancellationToken)
            .ContinueWith(task => !task.Result, cancellationToken);
    }
}