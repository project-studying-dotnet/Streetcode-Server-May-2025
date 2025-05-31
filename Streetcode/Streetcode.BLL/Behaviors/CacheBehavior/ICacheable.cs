namespace Streetcode.BLL.Behaviors;

public interface ICacheable
{
    string? CustomCacheKey { get; } 
    TimeSpan? AbsoluteExpiration { get; } 
}