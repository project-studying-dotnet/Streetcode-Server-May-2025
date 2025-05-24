using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.DTO.Media.Art;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Interfaces.BlobStorage;

namespace Streetcode.BLL.MediatR.Media.Art.GetAll;

public class GetAllArtsHandler : IRequestHandler<GetAllArtsQuery, Result<IEnumerable<ArtDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public GetAllArtsHandler(
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

    public async Task<Result<IEnumerable<ArtDTO>>> Handle(GetAllArtsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var arts = await _repositoryWrapper.ArtRepository.GetAllAsync(
                include: query => query
                    .Include(art => art.Image)
                        .ThenInclude(img => img.ImageDetails));

            if (arts == null || !arts.Any())
            {
                _logger.LogInformation("GetAllArtsHandler: No arts found.");
                return Result.Ok<IEnumerable<ArtDTO>>(new List<ArtDTO>());
            }

            var artDtos = _mapper.Map<IEnumerable<ArtDTO>>(arts);

            foreach (var artDto in artDtos)
            {
                if (artDto.Image != null && !string.IsNullOrWhiteSpace(artDto.Image.BlobName))
                {
                    artDto.Image.Base64 = await _blobService.FindFileInStorageAsBase64Async(artDto.Image.BlobName);
                }
            }

            _logger.LogInformation($"GetAllArtsHandler: Successfully retrieved {artDtos.Count()} arts.");
            return Result.Ok(artDtos);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(request, $"GetAllArtsHandler: An error occurred while retrieving arts. Error: {ex.Message}");
            return Result.Fail(new Error("Виникла помилка під час отримання списку мистецьких робіт."));
        }
    }
}