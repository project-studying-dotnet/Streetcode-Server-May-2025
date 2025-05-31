using System.Reflection;
using System.Text;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Streetcode.BLL.Behaviors;

public sealed class CacheBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>, ICacheable

{
    private readonly IDistributedCache _cache;
    
    public CacheBehavior(IDistributedCache cache)
    {
        _cache = cache;
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
        
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
            {
                DefaultMembersSearchFlags = BindingFlags.NonPublic | BindingFlags.Instance
            }
        };
        
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
        
        return response;
    }
    
    private string GenerateCacheKey<TRequest>(TRequest request)
    {
        var typeName = typeof(TRequest).Name;
        var props = typeof(TRequest).GetProperties();

        if (!props.Any())
        {
            return $"Cache:{typeName}";
        }

        var keyBuilder = new StringBuilder($"Cache:{typeName}");

        foreach (var prop in props.OrderBy(p => p.Name))
        {
            var value = prop.GetValue(request);
            
            var valueStr = value?.ToString() ?? "null";

            keyBuilder.Append($"|{prop.Name}:{valueStr}");
        }

        return keyBuilder.ToString();
    }
}