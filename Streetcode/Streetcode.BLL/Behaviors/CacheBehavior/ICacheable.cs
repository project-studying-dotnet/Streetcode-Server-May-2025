namespace Streetcode.BLL.Behaviors;

public interface ICacheable
{
    string CacheSetKey { get; set; }
    string? CustomCacheKey { get; } 
    TimeSpan? AbsoluteExpiration { get; } 
}