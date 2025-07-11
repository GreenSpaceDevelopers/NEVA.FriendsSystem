using Application.Common.Models;
using Application.Dtos.Requests.Shared;
using Domain.Models.Users;

namespace Application.Abstractions.Persistence.Repositories.Users;

public record UserWithBlockingInfo(ChatUser User, bool IsBlockedByMe, bool HasBlockedMe, bool IsFriendRequestSentByMe);

public record UserChatInfo(Guid? ChatId, bool IsChatDisabled, bool IsChatMuted);

public record OwnProfileData(ChatUser User, bool HasUnreadMessages, bool HasPendingFriendRequests);

public interface IChatUsersRepository : IBaseRepository<ChatUser>
{
    public Task<ChatUser?> GetByIdWithFriendsAsync(Guid requestUserId, CancellationToken cancellationToken);
    public Task<ChatUser?> GetByIdWithFriendsAsNoTrackingAsync(Guid requestUserId, CancellationToken cancellationToken);
    public Task<ChatUser?> GetByPersonalLinkWithFriendsAsync(string personalLink, CancellationToken cancellationToken);
    public Task<ChatUser?> GetByIdWithBlockerUsersAsync(Guid requestUserId, CancellationToken cancellationToken);
    public Task<bool> IsUsernameUniqueAsync(string username, CancellationToken cancellationToken = default);
    public Task<bool> IsPersonalLinkUniqueAsync(string personalLink, CancellationToken cancellationToken = default);
    public Task<ChatUser?> GetByIdWithProfileDataAsync(Guid userId, CancellationToken cancellationToken = default);
    public Task<PagedList<ChatUser>> GetBlockedUsersPagedAsync(Guid requestUserId, string? queryString, PageSettings requestPageSettings, CancellationToken cancellationToken = default);
    public Task<List<ChatUser>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    public Task<bool> IsUserBlockedByAsync(Guid userId, Guid potentialBlockerId, CancellationToken cancellationToken = default);
    public Task<PagedList<UserWithBlockingInfo>> SearchUsersWithBlockingInfoAsync(string? username, PageSettings requestPageSettings, Guid currentUserId, CancellationToken cancellationToken = default);
    public Task<List<UserWithBlockingInfo>> GetFriendsWithBlockingInfoAsync(Guid userId, string? searchQuery, CancellationToken cancellationToken = default);
    public Task<UserChatInfo?> GetChatInfoBetweenUsersAsync(Guid currentUserId, Guid targetUserId, CancellationToken cancellationToken = default);
    public Task<OwnProfileData?> GetOwnProfileDataAsync(Guid userId, CancellationToken cancellationToken = default);
}