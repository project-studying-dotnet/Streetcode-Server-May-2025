using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.Partners;

namespace Streetcode.BLL.MediatR.Partners.GetAll;

public record GetAllPartnersQuery : IRequest<Result<IEnumerable<PartnerDTO>>>, ICacheable
{
    public string CacheSetKey { get; set; } = Constants.CacheSetKeys.Partners;
    public string? CustomCacheKey => null;
    public TimeSpan? AbsoluteExpiration { get; } = TimeSpan.FromMinutes(10);
}