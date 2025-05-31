using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Analytics;

namespace Streetcode.BLL.MediatR.Analytics.Create;

public record CreateStatisticRecordCommand(StatisticRecordCreateDTO StatisticRecordCreateDTO) : IRequest<Result<StatisticRecordDTO>>;