using System.Reflection;
using System.Text;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Streetcode.BLL.Behaviors;

public sealed class CacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>, ICacheable
    where TResponse : ResultBase

{
    private readonly IDistributedCache _cache;
    private readonly IDatabase _redisDb;
    
    public CacheBehavior(IDistributedCache cache, IConnectionMultiplexer redis)
    {
        _cache = cache;
        _redisDb = redis.GetDatabase();
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (request is not ICacheable cacheable)
        {
            return await next(); 
        }
        
        var settings = new JsonSerializerSettings();
        var genericType = typeof(TResponse).GetGenericArguments().FirstOrDefault();
        if (genericType != null)
        {
            var converterType = typeof(ResultValueOnlyConverter<>).MakeGenericType(genericType);
            var converter = (JsonConverter)Activator.CreateInstance(converterType)!;
            settings.Converters.Add(converter);
        }
        
        string? cacheKey = GenerateCacheKey(request);
        
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var value = JsonConvert.DeserializeObject<TResponse>(cachedData, settings)!;
            return value;
        }
        
        var response = await next();
        
        var serializedResponse = JsonConvert.SerializeObject(response, settings)!;
        var expiration = cacheable.AbsoluteExpiration ?? TimeSpan.FromMinutes(5);
        
        await _cache.SetStringAsync(cacheKey, serializedResponse, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        },
        cancellationToken);
        
        await _redisDb.SetAddAsync(cacheable.CacheSetKey, cacheKey);
        
        return response;
    }
    
    private string GenerateCacheKey<TRequest>(TRequest request)
    {
        var cacheable = request as ICacheable; 
        var typeName = typeof(TRequest).Name;
        var key = $"{cacheable.CacheSetKey}:{typeName}";

        if (cacheable.CustomCacheKey != null)
        {
            key = $"{key}:{cacheable.CustomCacheKey}";
        }
        
        return key;
    }
}