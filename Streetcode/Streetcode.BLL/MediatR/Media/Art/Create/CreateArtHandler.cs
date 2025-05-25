using MediatR;
using FluentResults;
using Streetcode.DAL.Repositories.Interfaces.Base;
using AutoMapper;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Entities.Media;
using ImageEntity = Streetcode.DAL.Entities.Media.Images.Image;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.BLL.MediatR.Media.Art.Create
{
    public class CreateArtHandler : IRequestHandler<CreateArtCommand, Result<ArtDTO>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly IBlobService _blobService;
        private readonly ILoggerService _logger;

        public CreateArtHandler(
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

        public async Task<Result<ArtDTO>> Handle(CreateArtCommand request, CancellationToken cancellationToken)
        {
            var artRequest = request.ArtCreateRequest;

            if (artRequest.Image == null)
            {
                _logger.LogWarning("CreateArtCommand: Image data is null.");
                return Result.Fail(new Error("Дані зображення є обов'язковими."));
            }

            if (string.IsNullOrWhiteSpace(artRequest.Image.BaseFormat))
            {
                _logger.LogWarning("CreateArtCommand: Image BaseFormat (Base64) is null or empty.");
                return Result.Fail(new Error("Base64 рядок зображення є обов'язковим."));
            }

            if (string.IsNullOrWhiteSpace(artRequest.Image.Extension))
            {
                _logger.LogWarning("CreateArtCommand: Image Extension is null or empty.");
                return Result.Fail(new Error("Розширення файлу зображення є обов'язковим."));
            }

            try
            {
                string imageFileNameForBlob = string.IsNullOrWhiteSpace(artRequest.Image.Title)
                    ? Guid.NewGuid().ToString()
                    : artRequest.Image.Title;

                string hashBlobStorageName = await _blobService.SaveFileInStorageAsync(
                    artRequest.Image.BaseFormat,
                    imageFileNameForBlob,
                    artRequest.Image.Extension);

                var imageEntity = new ImageEntity
                {
                    BlobName = $"{hashBlobStorageName}.{artRequest.Image.Extension}",
                    MimeType = artRequest.Image.MimeType,
                    ImageDetails = (string.IsNullOrWhiteSpace(artRequest.Image.Alt) && string.IsNullOrWhiteSpace(artRequest.Image.Title))
                        ? null
                        : new ImageDetails
                        {
                            Title = artRequest.Image.Title,
                            Alt = artRequest.Image.Alt
                        }
                };

                await _repositoryWrapper.ImageRepository.CreateAsync(imageEntity);

                await _repositoryWrapper.SaveChangesAsync();

                var artEntity = new ArtEntity
                {
                    Title = artRequest.Title,
                    Description = artRequest.Description,
                    ImageId = imageEntity.Id
                };

                await _repositoryWrapper.ArtRepository.CreateAsync(artEntity);
                await _repositoryWrapper.SaveChangesAsync();

                var artDto = _mapper.Map<ArtDTO>(artEntity);

                if (artDto.Image != null && !string.IsNullOrWhiteSpace(imageEntity.BlobName))
                {
                    artDto.Image.Base64 = await _blobService.FindFileInStorageAsBase64Async(imageEntity.BlobName);
                    artDto.Image.MimeType = imageEntity.MimeType;

                    if (imageEntity.ImageDetails != null)
                    {
                        artDto.Image.ImageDetails = _mapper.Map<ImageDetailsDTO>(imageEntity.ImageDetails);
                    }
                }
                else if (artDto.Image == null && imageEntity.Id != 0)
                {
                    var createdImageDto = _mapper.Map<ImageDTO>(imageEntity);
                    if (!string.IsNullOrWhiteSpace(imageEntity.BlobName))
                    {
                        createdImageDto.Base64 = await _blobService.FindFileInStorageAsBase64Async(imageEntity.BlobName);
                    }

                    artDto.Image = createdImageDto;
                }

                _logger.LogInformation($"Art with ID {artEntity.Id} and Image ID {imageEntity.Id} created successfully.");
                return Result.Ok(artDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(request, $"Failed to create Art. Error: {ex.Message}");
                return Result.Fail(new Error($"Не вдалося створити об'єкт мистецтва: {ex.Message}"));
            }
        }
    }
}

