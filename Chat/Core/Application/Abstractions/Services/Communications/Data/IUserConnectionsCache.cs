namespace Application.Abstractions.Services.Communications.Data;

public interface IUserConnectionsCache
{
    public ValueTask AddOrUpdateAsync(string userId, string connectionId, CancellationToken cancellationToken = default);
    public Task RemoveAsync(string userId, string connectionId, CancellationToken cancellationToken = default);
    public Task<string?> GetConnectionIdAsync(string userId, CancellationToken cancellationToken = default);
    public Task<string[]> GetConnectionsForUsersAsync(Guid[] userIdsFromChat);
}