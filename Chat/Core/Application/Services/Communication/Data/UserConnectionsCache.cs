using System.Collections.Concurrent;
using Application.Abstractions.Services.Communications.Data;
using Application.Common.Constants;
using GS.CommonLibrary.Services;

namespace Application.Services.Communication.Data;

public class UserConnectionsCache(ICacheService cache) : IUserConnectionsCache
{
    public async ValueTask AddOrUpdateAsync(string userId, string connectionId, CancellationToken cancellationToken = default)
    {
        var key = FetchKeys.User + userId;
        
        if (await cache.KeyExistsAsync(key))
        {
            await cache.UpdateExpiryAsync(key, TimeSpan.FromMinutes(6));
            return;
        }
        
        await cache.StringSetAsync(key, connectionId, TimeSpan.FromMinutes(6));
    }

    public async Task RemoveAsync(string userId, string connectionId, CancellationToken cancellationToken = default)
    {
        var key = FetchKeys.User + userId;

        await cache.DeleteAsync(key);
    }

    public Task<string?> GetConnectionIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var key = FetchKeys.User + userId;

        return cache.StringGetAsync(key);
    }

    public async Task<string[]> GetConnectionsForUsersAsync(Guid[] userIdsFromChat)
    {
        var connections = new ConcurrentBag<string>();

        await Parallel.ForEachAsync(userIdsFromChat, async (userId, token) =>
        {
            var key = FetchKeys.User + userId;
            
            var connectionId = await cache.StringGetAsync(key);
            
            if (!string.IsNullOrEmpty(connectionId))
            {
                connections.Add(connectionId);
            }
        });
        
        return connections.ToArray();
    }
}