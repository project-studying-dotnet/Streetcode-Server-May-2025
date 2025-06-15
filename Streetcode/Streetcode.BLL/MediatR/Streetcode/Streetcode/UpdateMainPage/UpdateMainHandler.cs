using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.UpdateMainPage;

public class UpdateMainHandler : IRequestHandler<UpdateMainCommand, Result<Unit>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public UpdateMainHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<Unit>> Handle(UpdateMainCommand command, CancellationToken cancellationToken)
    {
        var streetcode = await _repositoryWrapper.StreetcodeRepository
            .GetFirstOrDefaultAsync(s => s.Id == command.StreetcodeMainPageDTO.Id);

        if (streetcode == null)
        {
            _logger.LogError(command, $"UpdateMainHandler: Streetcode with ID {command.StreetcodeMainPageDTO.Id} not found.");
            return Result.Fail($"Streetcode with ID {command.StreetcodeMainPageDTO.Id} not found.");
        }

        _mapper.Map(command.StreetcodeMainPageDTO, streetcode);

        _repositoryWrapper.StreetcodeRepository.Update(streetcode);

        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (!resultIsSuccess)
        {
            return Result.Fail("Failed to update the streetcode.");
        }

        _logger.LogInformation($"UpdateMainHandler: Streetcode with ID {command.StreetcodeMainPageDTO.Id} updated successfully.");
        return Result.Ok(Unit.Value);
    }
}
