﻿using FluentResults;
using MediatR;
using Streetcode.BLL.Behaviors;
using Streetcode.BLL.DTO.Team;

namespace Streetcode.BLL.MediatR.Team.GetById;

public record GetByIdTeamQuery(int Id) : IRequest<Result<TeamMemberDTO>>;