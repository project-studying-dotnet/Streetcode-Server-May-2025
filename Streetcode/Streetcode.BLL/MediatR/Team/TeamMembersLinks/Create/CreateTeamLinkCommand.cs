using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Team;

namespace Streetcode.BLL.MediatR.Team.TeamMembersLinks.Create;

public record CreateTeamLinkCommand(TeamMemberLinkDTO teamMember) : IRequest<Result<TeamMemberLinkDTO>>;