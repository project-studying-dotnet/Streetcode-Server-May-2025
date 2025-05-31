using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.Partners;

namespace Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;

public record GetPartnersByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<IEnumerable<PartnerDTO>>>, ICacheable
{
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(10);
    public string? CustomCacheKey => null;
}