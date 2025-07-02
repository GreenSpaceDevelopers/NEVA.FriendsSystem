using Application.Abstractions.Persistence.Repositories.Users;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Users;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Users;

public class ChatChatUsersRepository(ChatsDbContext dbContext) : BaseRepository<ChatUser>(dbContext), IChatUsersRepository
{
    public Task<PagedList<ChatUser>> GetByUsernamePagedAsync(string? username, PageSettings requestPageSettings, Guid currentUserId, IEnumerable<Guid> blockedUserIds, CancellationToken cancellationToken = default)
    {
        IQueryable<ChatUser> query = dbContext.Set<ChatUser>()
            .Where(user => user.Id != currentUserId)
            .Where(user => !blockedUserIds.Contains(user.Id))
            .OrderBy(user => user.Username);

        if (!string.IsNullOrEmpty(username))
        {
            query = query.Where(user => user.Username.StartsWith(username));
        }
        
        return query
            .ToPagedList(requestPageSettings, cancellationToken);
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
        IQueryable<ChatUser> query = dbContext.Set<ChatUser>()
            .Where(u => u.BlockedUsers.Any(b => b.Id == requestUserId))
            .Include(u => u.AspNetUser)
            .Include(u => u.Avatar)
            .OrderBy(u => u.Username);

        if (!string.IsNullOrEmpty(queryString))
        {
            query = query.Where(u => u.Username.Contains(queryString));
        }

        return query
            .Skip(requestPageSettings.Skip)
            .Take(requestPageSettings.Take)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .AnyAsync(user => user.Username == username, cancellationToken)
            .ContinueWith(task => !task.Result, cancellationToken);
    }

    public Task<ChatUser?> GetByIdWithProfileDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .Include(user => user.Avatar)
            .Include(user => user.Cover)
            .Include(user => user.PrivacySettings)
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public Task<PagedList<ChatUser>> GetBlockedUsersPagedAsync(Guid requestUserId, string? queryString, PageSettings requestPageSettings, CancellationToken cancellationToken = default)
    {
        IQueryable<ChatUser> query = dbContext.Set<ChatUser>()
            .Where(u => u.BlockedUsers.Any(b => b.Id == requestUserId))
            .Include(u => u.AspNetUser)
            .Include(u => u.Avatar);

        if (!string.IsNullOrEmpty(queryString))
        {
            query = query.Where(u => u.Username.Contains(queryString));
        }

        return query
            .OrderBy(u => u.Username)
            .ToPagedList(requestPageSettings, cancellationToken);
    }

    public Task<List<ChatUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }
}