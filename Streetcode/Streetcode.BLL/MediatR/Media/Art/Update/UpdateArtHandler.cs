using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;

namespace Streetcode.BLL.MediatR.Media.Art.Update;

public class UpdateArtHandler : IRequestHandler<UpdateArtCommand, Result<ArtDTO>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public UpdateArtHandler(
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

    public async Task<Result<ArtDTO>> Handle(UpdateArtCommand request, CancellationToken cancellationToken)
    {
        var artUpdateRequest = request.ArtUpdateRequest;

        // Removed try block

        var artEntity = await _repositoryWrapper.ArtRepository.GetFirstOrDefaultAsync(
            predicate: a => a.Id == artUpdateRequest.Id,
            include: query => query
                .Include(a => a.Image)
                    .ThenInclude(img => img.ImageDetails));

        if (artEntity == null)
        {
            string errorMsg = $"Art with ID {artUpdateRequest.Id} not found.";
            _logger.LogWarning($"UpdateArtHandler: {errorMsg}");
            return Result.Fail(new Error(errorMsg));
        }

        artEntity.Title = artUpdateRequest.Title;
        artEntity.Description = artUpdateRequest.Description;

        _repositoryWrapper.ArtRepository.Update(artEntity);
        var saveResult = await _repositoryWrapper.SaveChangesAsync();

        if (saveResult <= 0)
        {
            string errorMsg = $"Failed to update Art with ID {artUpdateRequest.Id}. No changes were saved to the database.";
            _logger.LogError(request, errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var updatedArtDto = _mapper.Map<ArtDTO>(artEntity);

        if (updatedArtDto.Image != null && !string.IsNullOrWhiteSpace(artEntity.Image?.BlobName))
        {
            updatedArtDto.Image.Base64 = await _blobService.FindFileInStorageAsBase64Async(artEntity.Image.BlobName);
            if (artEntity.Image != null)
            {
                updatedArtDto.Image.MimeType = artEntity.Image.MimeType;
                if (artEntity.Image.ImageDetails != null)
                {
                    updatedArtDto.Image.ImageDetails = _mapper.Map<ImageDetailsDTO>(artEntity.Image.ImageDetails);
                }
            }
        }

        _logger.LogInformation($"UpdateArtHandler: Art with ID {artEntity.Id} updated successfully.");
        return Result.Ok(updatedArtDto);
    }
}
