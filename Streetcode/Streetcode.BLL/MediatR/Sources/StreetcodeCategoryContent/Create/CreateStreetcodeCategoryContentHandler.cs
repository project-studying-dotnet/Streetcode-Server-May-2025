using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Create;

public class CreateStreetcodeCategoryContentHandler : IRequestHandler<CreateStreetcodeCategoryContentCommand, Result<StreetcodeCategoryContentDTO>>
{
    private readonly ILoggerService _loggerService;
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    
    public CreateStreetcodeCategoryContentHandler(
        ILoggerService loggerService,
        IMapper mapper,
        IRepositoryWrapper repositoryWrapper)
    {
        _loggerService = loggerService;
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<StreetcodeCategoryContentDTO>> Handle(CreateStreetcodeCategoryContentCommand request,
        CancellationToken cancellationToken)
    {
        var categoryContent = _mapper.Map<DAL.Entities.Sources.StreetcodeCategoryContent>(request.CategoryContentDto);
        
        var duplicate = await _repositoryWrapper
            .StreetcodeCategoryContentRepository
            .GetFirstOrDefaultAsync(
                predicate: c =>
                    c.StreetcodeId == categoryContent.StreetcodeId &&
                    c.SourceLinkCategoryId == categoryContent.SourceLinkCategoryId);

        if (duplicate != null)
        {
            const string errorMessage = "A Category with the same content already exists for this streetcode.";
            _loggerService.LogError(request.CategoryContentDto, errorMessage);
            return Result.Fail<StreetcodeCategoryContentDTO>(errorMessage);
        }
        
        await _repositoryWrapper.StreetcodeCategoryContentRepository.CreateAsync(categoryContent);
        var isSuccessResult = await _repositoryWrapper.SaveChangesAsync() > 0;

        if (isSuccessResult)
        {
            var result = _mapper.Map<StreetcodeCategoryContentDTO>(categoryContent);
            return Result.Ok(result);
        }
        else
        {
            const string errorMessage = "Failed to save the category content.";
            _loggerService.LogError(request.CategoryContentDto, errorMessage);
            return Result.Fail<StreetcodeCategoryContentDTO>(errorMessage);
        }
    }
}