using AutoMapper;
using FluentResults;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Audio;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Media.Audio.Update;

public class UpdateAudioHandler
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public UpdateAudioHandler(
        IRepositoryWrapper repositoryWrapper,
        IMapper mapper,
        IBlobService blobService,
        ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<Result<AudioDTO>> Handle(UpdateAudioCommand request, CancellationToken cancellationToken)
    {
        var audioDto = request.audioDTO;

        var audioEntity = await _repositoryWrapper.AudioRepository.GetFirstOrDefaultAsync(
            predicate: a => a.Id == audioDto.Id);

        if (audioEntity is null)
        {
            string errorMsg = $"Audio with ID {audioDto.Id} not found.";
            _logger.LogWarning($"UpdateAudioHandler: {errorMsg}");
            return Result.Fail(new Error(errorMsg));
        }

        audioEntity.Title = audioDto.Description;
        audioEntity.BlobName = audioDto.BlobName;

        _repositoryWrapper.AudioRepository.Update(audioEntity);
        var saveResult = await _repositoryWrapper.SaveChangesAsync();

        if (saveResult <= 0)
        {
            string errorMsg = $"Failed to update Audio with ID {audioDto.Id}. No changes were saved.";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var updatedAudioDto = _mapper.Map<AudioDTO>(audioEntity);

        if (!string.IsNullOrWhiteSpace(audioEntity.BlobName))
        {
            updatedAudioDto.Base64 = await _blobService.FindFileInStorageAsBase64Async(audioEntity.BlobName);
        }

        _logger.LogInformation($"UpdateAudioHandler: Audio with ID {audioEntity.Id} updated successfully.");
        return Result.Ok(updatedAudioDto);
    }
}
