using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.GetById;

public record GetFactByIdQuery(int Id) : IRequest<Result<FactDTO>>, ICacheable
{
    public string CacheSetKey { get; set; } = Constants.CacheSetKeys.Facts;
    public string? CustomCacheKey { get; } = Id.ToString();
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(10);
}