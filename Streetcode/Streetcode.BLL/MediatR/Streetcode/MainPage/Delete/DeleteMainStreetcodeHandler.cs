using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.MainPage.Delete;

public class DeleteMainStreetcodeHandler : IRequestHandler<DeleteMainStreetcodeCommand, Result<StreetcodeMainPageDeleteDTO>>
{
    private readonly IRepositoryWrapper _repository;
    private readonly ILoggerService _logger;

    public DeleteMainStreetcodeHandler(IRepositoryWrapper repository, ILoggerService logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<StreetcodeMainPageDeleteDTO>> Handle(DeleteMainStreetcodeCommand request, CancellationToken cancellationToken)
    {
        var streetcode = await _repository.StreetcodeRepository.GetFirstOrDefaultAsync(s => s.Id == request.Dto.StreetcodeId);

        if (streetcode is null)
        {
            _logger.LogError(request.Dto, "Streetcode not found.");
            return Result.Fail("Streetcode not found.");
        }

        streetcode.BriefDescription = null;

        _repository.StreetcodeRepository.Update(streetcode);
        var result = await _repository.SaveChangesAsync();

        if (result == 0)
        {
            _logger.LogError(request.Dto, "Failed to remove brief description.");
            return Result.Fail("Failed to remove brief description.");
        }

        return Result.Ok(request.Dto);
    }
}