using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Analytics.Delete;

public record DeleteStatisticRecordCommand(int Id) : IRequest<Result<Unit>>;