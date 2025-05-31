using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Analytics;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Analytics;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Analytics.Create;

public class CreateStatisticRecordHandler : IRequestHandler<CreateStatisticRecordCommand, Result<StatisticRecordDTO>>
{
    private readonly IMediator _mediator;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;
    private readonly IMapper _mapper;

    public CreateStatisticRecordHandler(
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

    public async Task<Result<StatisticRecordDTO>> Handle(CreateStatisticRecordCommand request, CancellationToken cancellationToken)
    {
        var statisticRecordCreateDTO = request.StatisticRecordCreateDTO;

        if (statisticRecordCreateDTO == null)
        {
            _logger.LogWarning("CreateStatisticRecordCommand: StatisticRecordCreateDTO is null.");
            return Result.Fail(new Error("Statistic record data is required."));
        }

        var statisticRecord = _mapper.Map<StatisticRecord>(statisticRecordCreateDTO);
        await _repositoryWrapper.StatisticRecordRepository.CreateAsync(statisticRecord);
        await _repositoryWrapper.SaveChangesAsync();
        _logger.LogInformation("CreateStatisticRecordCommand: Statistic record created successfully.");
        var statisticRecordDTO = _mapper.Map<StatisticRecordDTO>(statisticRecord);
        return Result.Ok(statisticRecordDTO);
    }
}