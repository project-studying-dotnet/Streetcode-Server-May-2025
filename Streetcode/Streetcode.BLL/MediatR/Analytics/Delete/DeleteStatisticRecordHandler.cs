using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Analytics.Delete;

public class DeleteStatisticRecordHandler : IRequestHandler<DeleteStatisticRecordCommand, Result<Unit>>
{
    private readonly IMediator _mediator;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public DeleteStatisticRecordHandler(
        IMediator mediator,
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger,
        IMapper mapper)
    {
        _mediator = mediator;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Unit>> Handle(DeleteStatisticRecordCommand request, CancellationToken cancellationToken)
    {
        var statisticRecord = await _repositoryWrapper.StatisticRecordRepository
            .GetFirstOrDefaultAsync(sr => sr.Id == request.Id);

        if (statisticRecord is null)
        {
            _logger.LogWarning($"StatisticRecord with Id {request.Id} not found.");
            return Result.Fail(new Error($"StatisticRecord with Id {request.Id} not found."));
        }

        _repositoryWrapper.StatisticRecordRepository.Delete(statisticRecord);

        try
        {
            await _repositoryWrapper.SaveChangesAsync();
            _logger.LogInformation($"StatisticRecord with Id {request.Id} deleted successfully.");
            return Result.Ok(Unit.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(request, $"Error deleting StatisticRecord with Id {request.Id}: {ex.Message}");
            return Result.Fail(new Error($"Error deleting StatisticRecord: {ex.Message}"));
        }
    }
}