using Application.Abstractions.Persistence.Repositories.Users;
using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Users;
using Domain.Models.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories.Users;

public class ChatChatUsersRepository(ChatsDbContext dbContext) : BaseRepository<ChatUser>(dbContext), IChatUsersRepository
{
    public Task<ChatUser?> GetByIdWithFriendsAsync(Guid requestUserId, CancellationToken cancellationToken)
    {
        return dbContext.Set<ChatUser>()
            .AsSplitQuery()
            .Include(user => user.AspNetUser)
            .Include(user => user.Friends)
            .ThenInclude(f => f.Avatar)
            .Include(user => user.FriendRequests)
            .ThenInclude(fr => fr.Avatar)
            .Include(user => user.BlockedUsers)
            .Include(user => user.WaitingFriendRequests)
            .Include(user => user.Avatar)
            .Include(user => user.Cover)
            .Include(user => user.PrivacySettings)
            .SingleOrDefaultAsync(user => user.Id == requestUserId, cancellationToken: cancellationToken);
    }

    public Task<ChatUser?> GetByIdWithFriendsAsNoTrackingAsync(Guid requestUserId, CancellationToken cancellationToken)
    {
        return dbContext.Set<ChatUser>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(user => user.AspNetUser)
            .Include(user => user.Friends)
            .ThenInclude(f => f.Avatar)
            .Include(user => user.FriendRequests)
            .ThenInclude(fr => fr.Avatar)
            .Include(user => user.BlockedUsers)
            .Include(user => user.WaitingFriendRequests)
            .Include(user => user.Avatar)
            .Include(user => user.Cover)
            .Include(user => user.PrivacySettings)
            .SingleOrDefaultAsync(user => user.Id == requestUserId, cancellationToken: cancellationToken);
    }

    public Task<ChatUser?> GetByPersonalLinkWithFriendsAsync(string personalLink, CancellationToken cancellationToken)
    {
        return dbContext.Set<ChatUser>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(user => user.AspNetUser)
            .Include(user => user.Friends)
            .ThenInclude(f => f.Avatar)
            .Include(user => user.FriendRequests)
            .ThenInclude(fr => fr.Avatar)
            .Include(user => user.BlockedUsers)
            .Include(user => user.WaitingFriendRequests)
            .Include(user => user.Avatar)
            .Include(user => user.Cover)
            .Include(user => user.PrivacySettings)
            .SingleOrDefaultAsync(user => user.PersonalLink == personalLink, cancellationToken: cancellationToken);
    }

    public Task<ChatUser?> GetByIdWithBlockerUsersAsync(Guid requestUserId, CancellationToken cancellationToken)
    {
        return dbContext.Set<ChatUser>()
            .Include(user => user.BlockedUsers)
            .SingleOrDefaultAsync(user => user.Id == requestUserId, cancellationToken: cancellationToken);
    }

    public Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .AnyAsync(user => user.Username == username, cancellationToken)
            .ContinueWith(task => !task.Result, cancellationToken);
    }

    public Task<bool> IsPersonalLinkUniqueAsync(string personalLink, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .AnyAsync(user => user.PersonalLink == personalLink, cancellationToken)
            .ContinueWith(task => !task.Result, cancellationToken);
    }

    public Task<ChatUser?> GetByIdWithProfileDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .AsNoTracking()
            .Include(user => user.Avatar)
            .Include(user => user.Cover)
            .Include(user => user.PrivacySettings)
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
    }

    public async Task<PagedList<ChatUser>> GetBlockedUsersPagedAsync(
        Guid requestUserId,
        string? queryString,
        PageSettings requestPageSettings,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Set<ChatUser>()
            .Include(u => u.BlockedUsers)
            .ThenInclude(bu => bu.Avatar)
            .Include(u => u.BlockedUsers)
            .ThenInclude(bu => bu.AspNetUser)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.Id == requestUserId, cancellationToken);

        if (user == null)
            return new PagedList<ChatUser> { Data = new List<ChatUser>(), TotalCount = 0 };

        var query = user.BlockedUsers.AsQueryable();

        if (!string.IsNullOrEmpty(queryString))
        {
            query = query.Where(u => u.Username.Contains(queryString));
        }

        var totalCount = query.Count();
        var data = query
            .OrderBy(u => u.Username)
            .Skip(requestPageSettings.Skip)
            .Take(requestPageSettings.Take)
            .ToList();

        return new PagedList<ChatUser>
        {
            Data = data,
            TotalCount = totalCount
        };
    }

    public Task<List<ChatUser>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .OrderBy(u => u.Username)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsUserBlockedByAsync(Guid userId, Guid potentialBlockerId, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .Where(u => u.Id == potentialBlockerId)
            .AnyAsync(u => u.BlockedUsers.Any(b => b.Id == userId), cancellationToken);
    }

    public async Task<PagedList<UserWithBlockingInfo>> SearchUsersWithBlockingInfoAsync(string? username, PageSettings requestPageSettings, Guid currentUserId, CancellationToken cancellationToken = default)
    {
        var currentUserInfo = await dbContext.Set<ChatUser>()
            .Where(u => u.Id == currentUserId)
            .Select(u => new 
            {
                FriendIds = u.Friends.Select(f => f.Id).ToList(),
                BlockedUserIds = u.BlockedUsers.Select(b => b.Id).ToList(),
                WaitingFriendRequestIds = u.WaitingFriendRequests.Select(wr => wr.Id).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (currentUserInfo == null)
        {
            return new PagedList<UserWithBlockingInfo> { Data = new List<UserWithBlockingInfo>(), TotalCount = 0 };
        }

        var usersQuery = dbContext.Set<ChatUser>()
            .Include(user => user.Avatar)
            .Where(user => user.Id != currentUserId)
            .Where(user => user.BlockedUsers.All(b => b.Id != currentUserId))
            .Where(user => !currentUserInfo.FriendIds.Contains(user.Id))
            .Where(user => !currentUserInfo.BlockedUserIds.Contains(user.Id));

        if (!string.IsNullOrEmpty(username))
        {
            usersQuery = usersQuery.Where(user => user.Username.StartsWith(username));
        }

        var orderedUsersQuery = usersQuery.OrderBy(user => user.Username);
        var totalCount = await orderedUsersQuery.CountAsync(cancellationToken);
        
        var pagedUsers = await orderedUsersQuery
            .Skip(requestPageSettings.Skip)
            .Take(requestPageSettings.Take)
            .ToListAsync(cancellationToken);

        var result = new List<UserWithBlockingInfo>();
        foreach (var user in pagedUsers)
        {
            const bool isBlockedByMe = false;
            const bool hasBlockedMe = false;
            var isFriendRequestSentByMe = currentUserInfo.WaitingFriendRequestIds.Contains(user.Id);

            result.Add(new UserWithBlockingInfo(user, isBlockedByMe, hasBlockedMe, isFriendRequestSentByMe));
        }

        return new PagedList<UserWithBlockingInfo>
        {
            Data = result,
            TotalCount = totalCount
        };
    }

    public async Task<List<UserWithBlockingInfo>> GetFriendsWithBlockingInfoAsync(Guid userId, string? searchQuery, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Set<ChatUser>()
            .Include(user => user.Friends)
            .ThenInclude(friend => friend.Avatar)
            .Where(user => user.Id == userId)
            .SelectMany(user => user.Friends)
            .Select(friend => new
            {
                User = friend,
                IsBlockedByMe = dbContext.Set<ChatUser>()
                    .Where(cu => cu.Id == userId)
                    .Any(cu => cu.BlockedUsers.Any(bu => bu.Id == friend.Id)), // Current user blocked this friend
                HasBlockedMe = friend.BlockedUsers.Any(b => b.Id == userId) // This friend blocked current user
            });

        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(x => x.User.Username.Contains(searchQuery));
        }

        var result = await query
            .OrderBy(x => x.User.Username)
            .ToListAsync(cancellationToken);

        return result.Select(x => new UserWithBlockingInfo(
            x.User,
            x.IsBlockedByMe,
            x.HasBlockedMe,
            false
        )).ToList();
    }

    public async Task<UserChatInfo> GetChatInfoBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken = default)
    {
        var chat = await dbContext.Set<Chat>()
            .Where(c => c.Users.Count == 2 && 
                       c.Users.Any(u => u.Id == userId1) && 
                       c.Users.Any(u => u.Id == userId2))
            .FirstOrDefaultAsync(cancellationToken);

        if (chat == null)
        {
            return new UserChatInfo(null, false, false);
        }

        var userChatSettings = await dbContext.Set<UserChatSettings>()
            .Where(ucs => ucs.UserId == userId1 && ucs.ChatId == chat.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return new UserChatInfo(
            chat.Id,
            userChatSettings?.IsDisabled ?? false,
            userChatSettings?.IsMuted ?? false
        );
    }

    public async Task<OwnProfileData?> GetOwnProfileDataAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await dbContext.Set<ChatUser>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(user => user.AspNetUser)
            .Include(user => user.Friends)
            .ThenInclude(f => f.Avatar)
            .Include(user => user.FriendRequests)
            .ThenInclude(fr => fr.Avatar)
            .Include(user => user.BlockedUsers)
            .Include(user => user.WaitingFriendRequests)
            .Include(user => user.Avatar)
            .Include(user => user.Cover)
            .Include(user => user.PrivacySettings)
            .Include(user => user.ChatSettings)
            .ThenInclude(ucs => ucs.Chat)
            .ThenInclude(c => c.Messages)
            .Include(user => user.ChatSettings)
            .ThenInclude(ucs => ucs.LastReadMessage)
            .Where(user => user.Id == userId)
            .Select(user => new
            {
                User = user,
                HasUnreadMessages = user.ChatSettings
                    .Any(ucs => ucs.Chat.Messages
                        .Any(m => m.SenderId != userId && 
                                 (ucs.LastReadMessage == null || m.CreatedAt > ucs.LastReadMessage.CreatedAt))),
                HasPendingFriendRequests = user.FriendRequests.Any()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result == null ? null : new OwnProfileData(result.User, result.HasUnreadMessages, result.HasPendingFriendRequests);
    }

    public Task<List<ChatUser>> GetByIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        return dbContext.Set<ChatUser>()
            .Where(user => userIds.Contains(user.Id))
            .ToListAsync(cancellationToken);
    }
}