using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Analytics.Delete;

public class DeleteStatisticRecordHandler : IRequestHandler<DeleteStatisticRecordCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public DeleteStatisticRecordHandler(
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(DeleteStatisticRecordCommand request, CancellationToken cancellationToken)
    {
        var recordId = request.Id;
        var record = await _repositoryWrapper.StatisticRecordRepository
            .GetFirstOrDefaultAsync(r => r.Id == recordId);

        if (record == null)
        {
            string errorMsg = $"No statistic record found by entered Id - {recordId}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        _repositoryWrapper.StatisticRecordRepository.Delete(record);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (resultIsSuccess)
        {
            return Result.Ok(Unit.Value);
        }
        else
        {
            string errorMsg = $"Failed to delete statistic record with Id - {recordId}";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }
    }
}