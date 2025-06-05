using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.Services.Cache;

public class NoOpCacheInvalidatonService : ICacheInvalidationService
{
    public Task InvalidateCacheAsync(string cacheSetKey)
    {
        return Task.CompletedTask;
    }
}