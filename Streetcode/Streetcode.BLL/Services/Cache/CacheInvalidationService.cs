using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using Streetcode.BLL.Interfaces.Cache;

namespace Streetcode.BLL.Services.Cache;

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly IDatabase _redisDb;
    private readonly IDistributedCache _cache;

    public CacheInvalidationService(IConnectionMultiplexer redis, IDistributedCache cache)
    {
        _redisDb = redis.GetDatabase();
        _cache = cache;
    }
    
    public async Task InvalidateAllCacheAsync(string cacheSetKey)
    {
        var keys = await _redisDb.SetMembersAsync(cacheSetKey);
        
        foreach (var redisKey in keys)
        {
            await _cache.RemoveAsync(redisKey!);     
        }
        
        await _redisDb.KeyDeleteAsync(cacheSetKey);
    }
}