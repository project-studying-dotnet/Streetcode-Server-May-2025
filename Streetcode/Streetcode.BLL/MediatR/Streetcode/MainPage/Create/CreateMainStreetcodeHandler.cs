using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.MainPage.Create;
public class CreateMainStreetcodeHandler : IRequestHandler<CreateMainStreetcodeCommand, Result<StreetcodeMainPageCreateDTO>>
{
    private readonly IRepositoryWrapper _repository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public CreateMainStreetcodeHandler(IRepositoryWrapper repository, IMapper mapper, ILoggerService logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StreetcodeMainPageCreateDTO>> Handle(CreateMainStreetcodeCommand request, CancellationToken cancellationToken)
    {
        var streetcode = await _repository.StreetcodeRepository.GetFirstOrDefaultAsync(s => s.Id == request.Dto.StreetcodeId);

        if (streetcode is null)
        {
            _logger.LogError(request.Dto, "Streetcode not found.");
            return Result.Fail("Streetcode not found.");
        }

        streetcode.BriefDescription = request.Dto.BriefDescription;

        _repository.StreetcodeRepository.Update(streetcode);
        var result = await _repository.SaveChangesAsync();

        if (result == 0)
        {
            _logger.LogError(request.Dto, "Failed to save brief description.");
            return Result.Fail("Failed to save brief description.");
        }

        return Result.Ok(request.Dto);
    }
}