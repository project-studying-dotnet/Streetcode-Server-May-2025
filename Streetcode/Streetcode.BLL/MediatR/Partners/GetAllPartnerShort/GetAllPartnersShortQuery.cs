using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.Partners;

namespace Streetcode.BLL.MediatR.Partners.GetAllPartnerShort;

public record GetAllPartnersShortQuery : IRequest<Result<IEnumerable<PartnerShortDTO>>>, ICacheable
{
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(10);
    public string? CustomCacheKey => null;
}