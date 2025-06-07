namespace Streetcode.BLL.Interfaces.Cache;

public interface ICacheInvalidationService
{
    Task InvalidateCacheAsync(string cacheSetKey);
}