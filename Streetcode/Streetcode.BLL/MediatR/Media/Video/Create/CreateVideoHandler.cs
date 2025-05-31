using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using Streetcode.BLL.DTO.Media.Video;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

using Entity = Streetcode.DAL.Entities.Media.Video;

namespace Streetcode.BLL.MediatR.Media.Video.Create;

public class CreateVideoHandler : IRequestHandler<CreateVideoCommand, Result<VideoDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public CreateVideoHandler(
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper,
        ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VideoDTO>> Handle(CreateVideoCommand request, CancellationToken cancellationToken)
    {
        var newVideo = _mapper.Map<Entity>(request.CreateVideoRequest);

        var validation = VideoValidation(request, newVideo);
        if (validation != null)
        {
            return validation;
        }

        var entity = await _repositoryWrapper.VideoRepository.CreateAsync(newVideo);
        var resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;
        if (resultIsSuccess)
        {
            return Result.Ok(_mapper.Map<VideoDTO>(entity));
        }
        else
        {
            const string errorMsg = "Failed to create a Video.";
            return LogAndFail(request, errorMsg);
        }
    }

    private Result<VideoDTO>? VideoValidation(CreateVideoCommand request, Entity? newVideo)
    {
        if (newVideo == null)
        {
            const string errorMsg = "Cannot convert null to Video.";
            return LogAndFail(request, errorMsg);
        }

        if (newVideo.Title.Length > 100)
        {
            const string errorMsg = "Заголовок відео не може бути більше 100 символів.";
            return LogAndFail(request, errorMsg);
        }

        if (newVideo.Url.IsNullOrEmpty())
        {
            const string errorMsg = "Посилання на відео є обов'язковим.";
            return LogAndFail(request, errorMsg);
        }

        return null;
    }

    private Result<VideoDTO> LogAndFail(CreateVideoCommand request, string errorMsg)
    {
        _logger.LogError(request, errorMsg);
        return Result.Fail(errorMsg);
    }
}
