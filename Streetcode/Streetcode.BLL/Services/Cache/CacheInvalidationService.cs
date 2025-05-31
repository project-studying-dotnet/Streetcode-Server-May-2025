using StackExchange.Redis;
using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.Services.Cache;

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly IConnectionMultiplexer _redis;
    public async Task InvalidateAllCacheAsync()
    {
        const string cachePrefix = "Cache:";
        
        var endpoint = _redis.GetEndPoints().First();
        var server = _redis.GetServer(endpoint);
        var db = _redis.GetDatabase();

        foreach (var key in server.Keys(pattern: $"{cachePrefix}*"))
        {
            await db.KeyDeleteAsync(key);
        }
    }
}