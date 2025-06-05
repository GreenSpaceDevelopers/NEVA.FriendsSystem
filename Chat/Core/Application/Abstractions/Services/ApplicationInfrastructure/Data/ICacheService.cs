namespace Application.Abstractions.Services.ApplicationInfrastructure.Data;

public interface ICacheService
{
    public Task StringSetAsync(string key, string value, TimeSpan? expiry = null);
    public Task<string?> StringGetAsync(string key);
    public Task DeleteAsync(string key);
    public Task<bool> KeyExistsAsync(string key);
    public Task UpdateExpiryAsync(string key, TimeSpan expiry);
    public Task SetAddAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    public Task<T?> GetSetAsync<T>(string key, CancellationToken cancellationToken = default);
};