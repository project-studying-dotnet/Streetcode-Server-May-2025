using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.GetAll;

public record GetAllFactsQuery : IRequest<Result<IEnumerable<FactDTO>>>, ICacheable
{
    public string CacheSetKey { get; set; } = Constants.CacheSetKeys.Facts;
    public string? CustomCacheKey { get; } = $"{nameof(GetAllFactsQuery)}";
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(10);
}