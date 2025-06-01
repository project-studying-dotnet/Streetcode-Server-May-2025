using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Analytics;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Analytics.Create;

public class CreateStatisticRecordHandler : IRequestHandler<CreateStatisticRecordCommand, Result<Unit>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public CreateStatisticRecordHandler(
        IRepositoryWrapper repositoryWrapper,
        ILoggerService logger,
        IMapper mapper)
    {
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Unit>> Handle(CreateStatisticRecordCommand request, CancellationToken cancellationToken)
    {
        var statisticRecordCreateDTO = request.StatisticRecordCreateDTO;

        if (statisticRecordCreateDTO is null)
        {
            const string errorMsg = "Statistic record data is required.";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var statisticRecord = _mapper.Map<StatisticRecord>(statisticRecordCreateDTO);

        await _repositoryWrapper.StatisticRecordRepository.CreateAsync(statisticRecord);
        await _repositoryWrapper.SaveChangesAsync();

        _logger.LogInformation("CreateStatisticRecordCommand: Statistic record created successfully.");

        var statisticRecordDTO = _mapper.Map<StatisticRecordDTO>(statisticRecord);

        return Result.Ok();
    }
}