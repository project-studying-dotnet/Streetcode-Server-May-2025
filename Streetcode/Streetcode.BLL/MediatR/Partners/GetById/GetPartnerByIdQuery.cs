using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.Partners;

namespace Streetcode.BLL.MediatR.Partners.GetById;

public record GetPartnerByIdQuery(int Id) : IRequest<Result<PartnerDTO>>, ICacheable
{
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(10);
    public string? CustomCacheKey => null;
}