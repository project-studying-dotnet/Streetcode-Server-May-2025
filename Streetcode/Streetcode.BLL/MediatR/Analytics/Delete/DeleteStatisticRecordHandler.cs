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
        var record = await _repositoryWrapper.StatisticRecordRepository
            .GetFirstOrDefaultAsync(r => r.Id == request.Id);

        if (record == null)
        {
            var message = $"StatisticRecord with Id {request.Id} not found.";
            _logger.LogWarning(message);
            return Result.Fail(new Error(message));
        }

        _repositoryWrapper.StatisticRecordRepository.Delete(record);

        try
        {
            await _repositoryWrapper.SaveChangesAsync();
            _logger.LogInformation($"StatisticRecord with Id {request.Id} deleted successfully.");
            return Result.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error deleting StatisticRecord with Id {request.Id}: {ex.Message}";
            _logger.LogError(request, errorMessage);
            return Result.Fail(new Error(errorMessage));
        }
    }
}