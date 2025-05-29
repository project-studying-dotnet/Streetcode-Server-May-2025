using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Analytics;

namespace Streetcode.BLL.MediatR.Analytics;

public record CreateStatisticRecordCommand(StatisticRecordCreateDTO StatisticRecordCreateDTO) : IRequest<Result<StatisticRecordDTO>>;