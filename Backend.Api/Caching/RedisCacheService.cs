using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Backend.Api.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(
        IDistributedCache cache,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken)
    {
        try
        {
            var value = await _cache.GetStringAsync(
                key,
                cancellationToken);

            if (value is null)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Redis GET failed for key {CacheKey}. Falling back to source.",
                key);

            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(value);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(
                key,
                serialized,
                options,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Redis SET failed for key {CacheKey}.",
                key);
        }
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken)
    {
        try
        {
            await _cache.RemoveAsync(
                key,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Redis DELETE failed for key {CacheKey}.",
                key);
        }
    }
}