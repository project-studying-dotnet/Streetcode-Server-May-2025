using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Analytics.Delete;

public record DeleteStatisticRecordCommand(int Id) : IRequest<Result<Unit>>;