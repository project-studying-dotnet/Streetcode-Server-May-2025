using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Video;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.ResultVariations;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.DAL.Specifications.Streetcode;
using Streetcode.DAL.Specifications.Video;

namespace Streetcode.BLL.MediatR.Media.Video.GetByStreetcodeId;

public class GetVideoByStreetcodeIdHandler : IRequestHandler<GetVideoByStreetcodeIdQuery, Result<VideoDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetVideoByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VideoDTO>> Handle(GetVideoByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var videoSpec = new VideoByStreetcodeIdSpec(request.StreetcodeId);
        var video = await _repositoryWrapper.VideoRepository.GetBySpecAsync(videoSpec, cancellationToken);

        if (video == null)
        {
            var streetcodeSpec = new StreetcodeByIdSpec(request.StreetcodeId);
            var streetcode = await _repositoryWrapper.StreetcodeRepository.GetBySpecAsync(streetcodeSpec, cancellationToken);

            if (streetcode is null)
            {
                string errorMsg = $"Streetcode with id: {request.StreetcodeId} doesn`t exist";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }

        NullResult<VideoDTO> result = new NullResult<VideoDTO>();
        result.WithValue(_mapper.Map<VideoDTO>(video));
        return result;
    }
}