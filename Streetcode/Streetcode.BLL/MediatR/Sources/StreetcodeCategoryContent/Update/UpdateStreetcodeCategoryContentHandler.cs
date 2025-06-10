using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;

public class UpdateStreetcodeCategoryContentHandler : IRequestHandler<UpdateStreetcodeCategoryContentCommand, Result<StreetcodeCategoryContentDTO>>
{
    private readonly IRepositoryWrapper _repository;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public UpdateStreetcodeCategoryContentHandler(
        IRepositoryWrapper repository,
        IMapper mapper,
        ILoggerService logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StreetcodeCategoryContentDTO>> Handle(UpdateStreetcodeCategoryContentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _repository.StreetcodeCategoryContentRepository.GetFirstOrDefaultAsync(
            predicate: s => s.Id == request.Dto.Id);

        if (entity is null)
        {
            const string errorMsg = "CategoryContent not found";
            _logger.LogError(request.Dto, errorMsg);
            return Result.Fail<StreetcodeCategoryContentDTO>(errorMsg);
        }

        if (request.Dto.Text != entity.Text)
        {
            entity.Text = request.Dto.Text;
        }

        if (request.Dto.SourceLinkCategoryId != entity.SourceLinkCategoryId)
        {
            entity.SourceLinkCategoryId = request.Dto.SourceLinkCategoryId;
        }

        if (request.Dto.StreetcodeId != entity.StreetcodeId)
        {
            entity.StreetcodeId = request.Dto.StreetcodeId;
        }

        _repository.StreetcodeCategoryContentRepository.Update(entity);
        var result = await _repository.SaveChangesAsync() > 0;

        if (!result)
        {
            const string errorMsg = "Update failed";
            _logger.LogError(request.Dto, errorMsg);
            return Result.Fail<StreetcodeCategoryContentDTO>(errorMsg);
        }

        var mapped = _mapper.Map<StreetcodeCategoryContentDTO>(entity);
        return Result.Ok(mapped);
    }
}