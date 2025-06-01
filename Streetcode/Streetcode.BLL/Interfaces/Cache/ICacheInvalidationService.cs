namespace Streetcode.BLL.Interfaces.Cache;

public interface ICacheInvalidationService
{
    Task InvalidateAllCacheAsync(string cacheSetKey);
}