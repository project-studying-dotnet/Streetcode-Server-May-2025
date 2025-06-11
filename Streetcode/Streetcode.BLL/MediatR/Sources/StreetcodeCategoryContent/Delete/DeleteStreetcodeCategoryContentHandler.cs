using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete;
public class DeleteStreetcodeCategoryContentHandler : IRequestHandler<DeleteStreetcodeCategoryContentCommand, Result<StreetcodeCategoryContentDTO>>
{
    private readonly ILoggerService _loggerService;
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;

    public DeleteStreetcodeCategoryContentHandler(
        ILoggerService loggerService,
        IMapper mapper,
        IRepositoryWrapper repositoryWrapper)
    {
        _loggerService = loggerService;
        _mapper = mapper;
        _repositoryWrapper = repositoryWrapper;
    }

    public async Task<Result<StreetcodeCategoryContentDTO>> Handle(DeleteStreetcodeCategoryContentCommand request, CancellationToken cancellationToken)
    {
        var streetcodeCategoryContent = await _repositoryWrapper.StreetcodeCategoryContentRepository
            .GetFirstOrDefaultAsync(t => t.Id == request.Id);

        if (streetcodeCategoryContent is null)
        {
            const string errorMsg = "Timeline item not found";
            _loggerService.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        _repositoryWrapper.StreetcodeCategoryContentRepository.Delete(streetcodeCategoryContent);
        var result = await _repositoryWrapper.SaveChangesAsync();

        if (result == 0)
        {
            const string errorMsg = "Failed to delete streetcodeCategoryContent.";
            _loggerService.LogError(request, errorMsg);
            return Result.Fail(errorMsg);
        }

        return Result.Ok(_mapper.Map<StreetcodeCategoryContentDTO>(streetcodeCategoryContent));
    }
}