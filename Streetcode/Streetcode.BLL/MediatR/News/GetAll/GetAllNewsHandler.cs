using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.News.GetAll;

public class GetAllNewsHandler : IRequestHandler<GetAllNewsQuery, Result<IEnumerable<NewsDTO>>>
{
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IMapper _mapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;
    private readonly IStringLocalizer<GetAllNewsHandler> _localizer;

    public GetAllNewsHandler(IRepositoryWrapper repositoryWrapper,
        IMapper mapper,
        IBlobService blobService, 
        ILoggerService logger,
        IStringLocalizer<GetAllNewsHandler> localizer)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _blobService = blobService;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result<IEnumerable<NewsDTO>>> Handle(GetAllNewsQuery request, CancellationToken cancellationToken)
    {
        var news = await _repositoryWrapper.NewsRepository.GetAllAsync(
            include: cat => cat.Include(img => img.Image));
        if (news == null)
        {
            var errorMsg = _localizer["CannotConvertNullToNews"];
            _logger.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        var newsDTOs = _mapper.Map<IEnumerable<NewsDTO>>(news);

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