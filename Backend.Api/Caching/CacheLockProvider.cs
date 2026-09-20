using System.Collections.Concurrent;

namespace Backend.Api.Caching;

public class CacheLockProvider
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public SemaphoreSlim GetLock(string key)
    {
        return _locks.GetOrAdd(
            key,
            _ => new SemaphoreSlim(1, 1));
    }
}