namespace Backend.Api.Caching;

public interface IDistributedLockService
{
    Task<bool> AcquireAsync(
        string key,
        string token,
        TimeSpan expiration);

    Task<bool> ReleaseAsync(
        string key,
        string token);
}