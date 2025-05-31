using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.Team;

namespace Streetcode.BLL.MediatR.Team.GetAll;

public record GetAllTeamQuery : IRequest<Result<IEnumerable<TeamMemberDTO>>>, ICacheable
{
    public string? CustomCacheKey { get; set; } = null;
    public TimeSpan? AbsoluteExpiration { get; set; } = TimeSpan.FromMinutes(5);
}