﻿using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.Interfaces.Logging;

namespace Streetcode.BLL.MediatR.News.GetById;

public class GetNewsByIdHandler : IRequestHandler<GetNewsByIdQuery, Result<NewsDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly IBlobService _blobService;
    private readonly ILoggerService _logger;
    private readonly IStringLocalizer<GetNewsByIdHandler> _localizer;

    public GetNewsByIdHandler(IMapper mapper,
        IRepositoryWrapper repositoryWrapper,
        IBlobService blobService,
        ILoggerService logger,
        IStringLocalizer<GetNewsByIdHandler> localizer)
    {
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
        _blobService = blobService;
        _logger = logger;
        _localizer = localizer;
    }

    public async Task<Result<NewsDTO>> Handle(GetNewsByIdQuery request, CancellationToken cancellationToken)
    {
        int id = request.id;
        var newsDTO = _mapper.Map<NewsDTO>(await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
            predicate: sc => sc.Id == id,
            include: scl => scl
                .Include(sc => sc.Image)));
        if (newsDTO is null)
        {
            var errorMsg = _localizer["NoNewsByEnteredId", id];
            _logger.LogError(request, errorMsg);

            return Result.Fail(errorMsg);
        }

        if (newsDTO.Image is not null)
        {
            newsDTO.Image.Base64 = await _blobService.FindFileInStorageAsBase64Async(newsDTO.Image.BlobName);
        }

        return Result.Ok(newsDTO);
    }
}