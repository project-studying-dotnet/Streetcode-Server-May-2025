using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Microsoft.EntityFrameworkCore;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Interfaces.Logging;

namespace Streetcode.BLL.MediatR.News.SortedByDateTime;

public class SortedByDateTimeHandler : IRequestHandler<SortedByDateTimeQuery, Result<List<NewsDTO>>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;

    public SortedByDateTimeHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, IBlobService blobService, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
        _logger = logger;
    }

    public async Task<Result<List<NewsDTO>>> Handle(SortedByDateTimeQuery request, CancellationToken cancellationToken)
    {
        var news = await _repositoryWrapper.NewsRepository.GetAllAsync(
            include: cat => cat.Include(img => img.Image));
        if (news == null)
        {
            const string errorMsg = "There are no news in the database";
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        var newsDTOs = _mapper.Map<IEnumerable<NewsDTO>>(news).OrderByDescending(x => x.CreationDate).ToList();

        var tasks = newsDTOs
            .Where(dto => dto.Image is not null)
            .Select(async dto =>
            {
                dto.Image!.Base64 = await _blobService.FindFileInStorageAsBase64Async(dto.Image.BlobName!);
                return dto;
            })
            .ToList();

        var processedNewsDTOs = await Task.WhenAll(tasks);

        return Result.Ok(newsDTOs);
    }
}