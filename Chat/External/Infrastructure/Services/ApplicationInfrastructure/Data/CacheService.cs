using Application.Abstractions.Services.ApplicationInfrastructure.Data;
using MessagePack;
using StackExchange.Redis;

namespace Infrastructure.Services.ApplicationInfrastructure.Data;

public class CacheService(IDatabase cache, string fetchKey) : ICacheService
{
    public Task StringSetAsync(string key, string value, TimeSpan? expiry = null)
    {
        return cache.StringSetAsync(fetchKey + key, value, expiry);
    }

    public async Task<string?> StringGetAsync(string key)
    {
        var value = await cache.StringGetAsync(fetchKey + key);
        
        return value;
    }

    public Task DeleteAsync(string key)
    {
        return cache.KeyDeleteAsync(fetchKey + key);
    }

    public Task<bool> KeyExistsAsync(string key)
    {
        return cache.KeyExistsAsync(fetchKey + key);
    }

    public Task UpdateExpiryAsync(string key, TimeSpan expiry)
    {
        return cache.KeyExpireAsync(fetchKey + key, expiry);
    }

    public Task SetAddAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        var body = MessagePackSerializer.Serialize(value, cancellationToken: cancellationToken);

        return cache.SetAddAsync(fetchKey + key, body);
    }

    public async Task<T?> GetSetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var body = await cache.StringGetAsync(fetchKey + key);
        
        return body.IsNullOrEmpty 
            ? default 
            : MessagePackSerializer.Deserialize<T>(body, cancellationToken: cancellationToken);
    }
}