using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.BLL.MediatR.Media.Art.Create;
public class CreateArtHandler : IRequestHandler<CreateArtCommand, Result<ArtDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public CreateArtHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, ILoggerService logger)
    {
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
        _logger = logger;
    }

    public async Task<Result<ArtDTO>> Handle(CreateArtCommand request, CancellationToken cancellationToken)
    {
        var newArt = _mapper.Map<ArtEntity>(request.newArt);

        if (newArt is null)
        {
            const string errorMsg = "Cannot convert null to art";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        var entity = await _repositoryWrapper.ArtRepository.CreateAsync(newArt);

        var isSuccessResult = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (isSuccessResult)
        {
            return Result.Ok(_mapper.Map<ArtDTO>(entity));
        }
        else
        {
            const string errorMsg = "Failed to save art.";
            _logger.LogError(request, errorMsg);

            return Result.Fail(new Error(errorMsg));
        }
    }
}