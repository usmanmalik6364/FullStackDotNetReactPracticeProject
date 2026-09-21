using StackExchange.Redis;

namespace Backend.Api.Caching;

public class RedisDistributedLockService : IDistributedLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisDistributedLockService(
        IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> AcquireAsync(
        string key,
        string token,
        TimeSpan expiration)
    {
        var db = _redis.GetDatabase();

        return await db.LockTakeAsync(
            key,
            token,
            expiration);
    }

    public async Task<bool> ReleaseAsync(
        string key,
        string token)
    {
        var db = _redis.GetDatabase();

        return await db.LockReleaseAsync(
            key,
            token);
    }
}